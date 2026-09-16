using CK.Core;
using CK.SqlServer.UtilTests;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Shouldly;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static CK.Testing.SqlTransformTestHelper;

namespace CK.SqlServer.Parser.Tests;

[TestFixture]
[Category( "SqlAnalyser" )]
public class SqlAnalyserTest
{
    [Test]
    public void AdventureWorks2012_FullSchema_has_no_errors()
    {
        string text = TestHelper.LoadTextFromParsingScripts( "AdventureWorks2012-FullSchema.sql" );
        ISqlNode e;
        SqlAnalyser.ErrorResult r = SqlAnalyser.Parse( out e, ParseMode.OneOrMoreStatements, text );
        r.IsError.ShouldBeFalse( r.ToString() );
        e.ToString( true, true ).ReplaceLineEndings().ShouldBe( text );
        e.ChildrenNodes.All( n => n is ISqlStatement ).ShouldBeTrue();
    }

    [TestCase( "sp_GetDDL.sql", 7 )]
    [TestCase( "SQLDOM_core_persist_927.sql", 86 )]
    public void The_big_scripts_are_correctlty_parsed( string name, int numberOfStatement )
    {
        string text = TestHelper.LoadTextFromParsingScripts( name );
        ISqlNode e;
        SqlAnalyser.ErrorResult r = SqlAnalyser.Parse( out e, ParseMode.Script, text );
        r.IsError.ShouldBeFalse( r.ToString() );
        e.ToString( true, true ).ReplaceLineEndings().ShouldBe( text );

        XElement visited = new SqlToXmlStatementVisitor().ToXml( "Statements", e );
        string visitedString = visited.ToString();
        TestHelper.Monitor.Trace( visitedString );
        if( numberOfStatement != -1 )
        {
            ((SqlStatementList)e).Count.ShouldBe( numberOfStatement );
        }
    }

    [TestCase( "Opt.1.sql" )]
    [TestCase( "Opt.2.sql" )]
    [TestCase( "Opt.3.sql" )]
    [TestCase( "Opt.4.sql" )]
    [TestCase( "Opt.5.sql" )]
    [TestCase( "Opt.6.sql" )]
    public void parsing_multiple_sp( string name )
    {
        var texts = Regex.Split( TestHelper.LoadTextFromParsingScripts( name ), "^\\s*GO", RegexOptions.Multiline );
        var a = new SqlAnalyser();
        ISqlServerStoredProcedure last = null;
        ISqlStatement p;
        foreach( var text in texts )
        {
            a.Reset( text );
            while( (p = a.IsExtendedStatement( false )) != null )
            {
                var proc = p as ISqlServerStoredProcedure;
                if( proc == null )
                {
                    using( TestHelper.Monitor.OpenError( "Found a " + p.GetType().Name ) )
                    {
                        TestHelper.Monitor.Trace( p.ToString() );
                        proc.ShouldNotBeNull( "Found a " + p.GetType().Name );
                    }
                }
                last = proc;
                TestHelper.Monitor.Trace( "Success: " + proc.ToStringSignature( true ) );
            }
            var r = a.GetCurrentResult();
            if( r.IsError )
            {
                r.LogOnError( TestHelper.Monitor );
                break;
            }
        }
        (last != null && last.SchemaName == "CKParser.TheEnd").ShouldBeTrue( "Not all have been processed." );
    }

    [Test]
    public void checking_different_kind_of_parameters()
    {
        CheckStatement<SqlStoredProcedure>( "sStoredProcedureInputOutput.sql", sp =>
        {
            sp.FullName.Identifiers[0].ToString().ShouldBe( "CK" );
            sp.FullName.Identifiers[1].ToString().ShouldBe( "sStoredProcedureInputOutput" );
            sp.FullName.ToString().ShouldBe( "CK.sStoredProcedureInputOutput" );

            sp.Parameters[0].IsOutput.ShouldBeFalse();
            sp.Parameters[0].IsReadOnly.ShouldBeFalse();
            sp.Parameters[0].DefaultValue.ShouldBeNull();
            sp.Parameters[0].Variable.Identifier.IsVariable.ShouldBeTrue();
            sp.Parameters[0].Variable.Identifier.Name.ShouldBe( "@p1" );
            sp.Parameters[0].Variable.TypeDecl.DbType.ShouldBe( SqlDbType.Int );
            sp.Parameters[0].Variable.TypeDecl.SyntaxSize.ShouldBe( -2, "Size does not apply." );
            sp.Parameters[0].IsNotNull.ShouldBeFalse();

            sp.Parameters[1].IsOutput.ShouldBeFalse();
            sp.Parameters[1].IsReadOnly.ShouldBeFalse();
            sp.Parameters[1].DefaultValue.ShouldNotBeNull();
            sp.Parameters[1].DefaultValue.ToString().ShouldBe( "0" );
            sp.Parameters[1].Variable.Identifier.IsVariable.ShouldBeTrue();
            sp.Parameters[1].Variable.Identifier.Name.ShouldBe( "@p2" );
            sp.Parameters[1].Variable.TypeDecl.DbType.ShouldBe( SqlDbType.TinyInt );
            sp.Parameters[1].IsNotNull.ShouldBeTrue();

            sp.Parameters[2].IsOutput.ShouldBeTrue();
            sp.Parameters[2].IsReadOnly.ShouldBeFalse();
            sp.Parameters[2].DefaultValue.ShouldBeNull();
            sp.Parameters[2].Variable.Identifier.IsVariable.ShouldBeTrue();
            sp.Parameters[2].Variable.Identifier.Name.ShouldBe( "@p3" );
            sp.Parameters[2].Variable.TypeDecl.DbType.ShouldBe( SqlDbType.SmallInt );
            sp.Parameters[2].IsNotNull.ShouldBeTrue();

            sp.Parameters[3].IsOutput.ShouldBeFalse();
            sp.Parameters[3].IsReadOnly.ShouldBeFalse();
            sp.Parameters[3].DefaultValue.ToString().ShouldBe( "N'Murfn...'" );
            sp.Parameters[3].Variable.Identifier.IsVariable.ShouldBeTrue();
            sp.Parameters[3].Variable.Identifier.Name.ShouldBe( "@p4" );
            sp.Parameters[3].Variable.TypeDecl.DbType.ShouldBe( SqlDbType.NVarChar );
            sp.Parameters[3].Variable.TypeDecl.SyntaxSize.ShouldBe( 50 );
            sp.Parameters[3].IsNotNull.ShouldBeFalse();

            sp.Parameters[4].IsOutput.ShouldBeTrue();
            sp.Parameters[4].IsInputOutput.ShouldBeTrue();
            sp.Parameters[4].IsReadOnly.ShouldBeFalse();
            sp.Parameters[4].DefaultValue.ShouldBeNull();
            sp.Parameters[4].Variable.Identifier.IsVariable.ShouldBeTrue();
            sp.Parameters[4].Variable.Identifier.Name.ShouldBe( "@p5" );
            sp.Parameters[4].Variable.TypeDecl.DbType.ShouldBe( SqlDbType.VarChar );
            sp.Parameters[4].Variable.TypeDecl.SyntaxSize.ShouldBe( -1, "Size is max." );
            sp.Parameters[4].IsNotNull.ShouldBeFalse();

            sp.Parameters[5].IsOutput.ShouldBeTrue();
            sp.Parameters[5].IsInputOutput.ShouldBeTrue();
            sp.Parameters[5].IsReadOnly.ShouldBeFalse();
            sp.Parameters[5].DefaultValue.ShouldBeNull();
            sp.Parameters[5].Variable.Identifier.IsVariable.ShouldBeTrue();
            sp.Parameters[5].Variable.Identifier.Name.ShouldBe( "@p6" );
            sp.Parameters[5].Variable.TypeDecl.DbType.ShouldBe( SqlDbType.Char );
            sp.Parameters[5].Variable.TypeDecl.SyntaxSize.ShouldBe( 0, "Size is undefined." );
            sp.Parameters[5].IsNotNull.ShouldBeTrue();

            sp.Parameters[6].IsOutput.ShouldBeTrue();
            sp.Parameters[6].IsInputOutput.ShouldBeFalse( "--input behind the comma..." );
            sp.Parameters[6].IsReadOnly.ShouldBeFalse();
            sp.Parameters[6].DefaultValue.ShouldBeNull();
            sp.Parameters[6].Variable.Identifier.IsVariable.ShouldBeTrue();
            sp.Parameters[6].Variable.Identifier.Name.ShouldBe( "@p7" );
            sp.Parameters[6].Variable.TypeDecl.DbType.ShouldBe( SqlDbType.Xml );
            sp.Parameters[6].Variable.TypeDecl.SyntaxSize.ShouldBe( -2, "Size does not apply." );
            sp.Parameters[6].IsNotNull.ShouldBeFalse();

            sp.Parameters[7].IsOutput.ShouldBeTrue();
            sp.Parameters[7].IsInputOutput.ShouldBeTrue( "-- input on the line above." );
            sp.Parameters[7].IsReadOnly.ShouldBeFalse();
            sp.Parameters[7].DefaultValue.ShouldBeNull();
            sp.Parameters[7].Variable.Identifier.IsVariable.ShouldBeTrue();
            sp.Parameters[7].Variable.Identifier.Name.ShouldBe( "@p8" );
            sp.Parameters[7].Variable.TypeDecl.DbType.ShouldBe( SqlDbType.SmallDateTime );
            sp.Parameters[7].Variable.TypeDecl.SyntaxSize.ShouldBe( -2, "Size does not apply." );
            sp.Parameters[7].IsNotNull.ShouldBeFalse();

            sp.Parameters[8].IsOutput.ShouldBeFalse();
            sp.Parameters[8].IsInputOutput.ShouldBeFalse();
            sp.Parameters[8].IsReadOnly.ShouldBeFalse();
            sp.Parameters[8].DefaultValue.IsVariable.ShouldBeFalse();
            sp.Parameters[8].DefaultValue.IsNull.ShouldBeTrue();
            sp.Parameters[8].DefaultValue.IsLiteral.ShouldBeFalse();

            sp.Header.ToStringCompact().ShouldBe( "procedure CK.sStoredProcedureInputOutput @p1 int, @p2 tinyint /*not null*/=0, @p3 smallint /*not null*/output, @p4 nvarchar(50)=N'Murfn...', @p5 varchar(max) /*input*/output, @p6 char /*not null, input*/output, @p7 Xml output, @p8 smalldatetime /*input*/output, @p9 smalldatetime=null" );
        } );
    }

    [TestCase( "select §e.ProductCode from CK.tProducts §e group by §e.ProductCode" )]
    public void simple_dyn_fragment_test( string text )
    {
        var res = SqlAnalyser.Parse( out ISqlNode node, ParseMode.Statement, text );
        res.IsError.ShouldBeFalse();
        var selectNode = (SelectSpec)(((SqlSelectStatement)node).Select);
        var groupby = selectNode.GroupByClause;
        groupby.ShouldNotBeNull();
    }

    [DebuggerStepThrough]
    internal static T CheckStatement<T>( string fileName, Action<T> check ) where T : class, ISqlStatement
    {
        string text = TestHelper.LoadTextFromParsingScripts( fileName );
        T s = TestHelper.ParseOneStatementAndCheckString<T>( text, false );
        check( s );
        s = TestHelper.ParseOneStatementAndCheckString<T>( text, true );
        check( s );
        return s;
    }

    [Explicit]
    [TestCase( null )]
    public void parse_all_stored_procedures_from_database( string connectionString )
    {
        Assume.That( connectionString != null );
        using( var c = new SqlConnection( connectionString ) )
        {
            c.Open();
            using( var cmd = new SqlCommand( $@"
                            select s.name, p.name, OBJECT_DEFINITION(OBJECT_ID(s.name + '.' + p.name)) 
	                            from sys.procedures p
	                            inner join sys.schemas s on s.schema_id = p.schema_id", c ) )
            {
                ISqlServerParser parser = new SqlServerParser();
                using( var r = cmd.ExecuteReader() )
                {
                    while( r.Read() )
                    {
                        try
                        {
                            string schema = r.GetString( 0 );
                            string name = r.GetString( 1 );
                            string fullBody = r.GetString( 2 );
                            var result = parser.ParseStoredProcedure( fullBody );
                            if( result.IsError )
                            {
                                result.LogOnError( TestHelper.Monitor );
                                TestHelper.Monitor.Trace( fullBody );
                            }
                            else TestHelper.Monitor.Trace( "Successfuly parsed: " + result.Result.ToStringSignature( true ) );
                        }
                        catch( Exception ex )
                        {
                            TestHelper.Monitor.Fatal( ex );
                        }
                    }
                }
            }
        }

    }
}
