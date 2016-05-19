using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Xml.Linq;
using CK.Core;
using CK.SqlServer.UtilTests;
using System.Data.SqlClient;

namespace CK.SqlServer.Parser.Tests
{
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
            Assert.That( r.IsError, Is.False, r.ToString() );
            Assert.That( e.ToString( true, true ).NormalizeEOL(), Is.EqualTo( text ) );
            Assert.That( e.ChildrenNodes.All( n => n is ISqlStatement) );
        }

        [TestCase( "sp_GetDDL.sql", 7 )]
        [TestCase( "SQLDOM_core_persist_927.sql", 86 )]
        public void The_big_scripts_are_correctlty_parsed( string name, int numberOfStatement )
        {
            string text = TestHelper.LoadTextFromParsingScripts( name );
            ISqlNode e;
            SqlAnalyser.ErrorResult r = SqlAnalyser.Parse( out e, ParseMode.Script, text );
            Assert.That( r.IsError, Is.False, r.ToString() );
            Assert.That( e.ToString( true, true ).NormalizeEOL(), Is.EqualTo( text ) );

            XElement visited = new SqlToXmlStatementVisitor().ToXml( "Statements", e );
            string visitedString = visited.ToString();
            TestHelper.ConsoleMonitor.Trace().Send( visitedString );
            if( numberOfStatement != -1 )
            {
                Assert.That( ((SqlStatementList)e).Count, Is.EqualTo( numberOfStatement ) );
            }
        }

        [TestCase( "Optiform.1.sql" )]
        [TestCase( "Optiform.2.sql" )]
        [TestCase( "Optiform.3.sql" )]
        [TestCase( "Optiform.4.sql" )]
        [TestCase( "Optiform.5.sql" )]
        [TestCase( "Optiform.6.sql" )]
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
                        using( TestHelper.ConsoleMonitor.OpenError().Send( "Found a " + p.GetType().Name ) )
                        {
                            TestHelper.ConsoleMonitor.Trace().Send( p.ToString() );
                            Assert.Fail( "Found a " + p.GetType().Name );
                        }
                    }
                    last = proc;
                    TestHelper.ConsoleMonitor.Trace().Send( "Success: " + proc.ToStringSignature( true ) );
                }
                var r = a.GetCurrentResult();
                if( r.IsError )
                {
                    r.LogOnError( TestHelper.ConsoleMonitor );
                    break;
                }
            }
            Assert.That( last != null && last.SchemaName == "CKParser.TheEnd", "Not all have been processed." );
        }

        [Test]
        public void checking_different_kind_of_parameters()
        {
            CheckStatement<SqlStoredProcedure>( "sStoredProcedureInputOutput.sql", sp =>
            {
                Assert.That( sp.FullName.Identifiers[0].ToString(), Is.EqualTo( "CK" ) );
                Assert.That( sp.FullName.Identifiers[1].ToString(), Is.EqualTo( "sStoredProcedureInputOutput" ) );
                Assert.That( sp.FullName.ToString(), Is.EqualTo( "CK.sStoredProcedureInputOutput" ) );

                Assert.That( sp.Parameters[0].IsOutput, Is.False );
                Assert.That( sp.Parameters[0].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[0].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[0].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[0].Variable.Identifier.Name, Is.EqualTo( "@p1" ) );
                Assert.That( sp.Parameters[0].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Int ) );
                Assert.That( sp.Parameters[0].Variable.TypeDecl.SyntaxSize, Is.EqualTo( -2 ), "Size does not apply." );

                Assert.That( sp.Parameters[1].IsOutput, Is.False );
                Assert.That( sp.Parameters[1].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[1].DefaultValue, Is.Not.Null );
                Assert.That( sp.Parameters[1].DefaultValue.ToString(), Is.EqualTo( "0" ) );
                Assert.That( sp.Parameters[1].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[1].Variable.Identifier.Name, Is.EqualTo( "@p2" ) );
                Assert.That( sp.Parameters[1].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.TinyInt ) );

                Assert.That( sp.Parameters[2].IsOutput, Is.True );
                Assert.That( sp.Parameters[2].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[2].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[2].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[2].Variable.Identifier.Name, Is.EqualTo( "@p3" ) );
                Assert.That( sp.Parameters[2].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.SmallInt ) );

                Assert.That( sp.Parameters[3].IsOutput, Is.False );
                Assert.That( sp.Parameters[3].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[3].DefaultValue.ToString(), Is.EqualTo( "N'Murfn...'" ) );
                Assert.That( sp.Parameters[3].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[3].Variable.Identifier.Name, Is.EqualTo( "@p4" ) );
                Assert.That( sp.Parameters[3].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.NVarChar ) );
                Assert.That( sp.Parameters[3].Variable.TypeDecl.SyntaxSize, Is.EqualTo( 50 ) );

                Assert.That( sp.Parameters[4].IsOutput, Is.True );
                Assert.That( sp.Parameters[4].IsInputOutput, Is.True );
                Assert.That( sp.Parameters[4].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[4].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[4].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[4].Variable.Identifier.Name, Is.EqualTo( "@p5" ) );
                Assert.That( sp.Parameters[4].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.VarChar ) );
                Assert.That( sp.Parameters[4].Variable.TypeDecl.SyntaxSize, Is.EqualTo( -1 ), "Size is max." );

                Assert.That( sp.Parameters[5].IsOutput, Is.True );
                Assert.That( sp.Parameters[5].IsInputOutput, Is.True );
                Assert.That( sp.Parameters[5].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[5].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[5].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[5].Variable.Identifier.Name, Is.EqualTo( "@p6" ) );
                Assert.That( sp.Parameters[5].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Char ) );
                Assert.That( sp.Parameters[5].Variable.TypeDecl.SyntaxSize, Is.EqualTo( 0 ), "Size is undefined." );

                Assert.That( sp.Parameters[6].IsOutput, Is.True );
                Assert.That( sp.Parameters[6].IsInputOutput, Is.False, "--input behind the comma..." );
                Assert.That( sp.Parameters[6].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[6].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[6].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[6].Variable.Identifier.Name, Is.EqualTo( "@p7" ) );
                Assert.That( sp.Parameters[6].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Xml ) );
                Assert.That( sp.Parameters[6].Variable.TypeDecl.SyntaxSize, Is.EqualTo( -2 ), "Size does not apply." );

                Assert.That( sp.Parameters[7].IsOutput, Is.True );
                Assert.That( sp.Parameters[7].IsInputOutput, Is.True, "-- input on the line above." );
                Assert.That( sp.Parameters[7].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[7].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[7].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[7].Variable.Identifier.Name, Is.EqualTo( "@p8" ) );
                Assert.That( sp.Parameters[7].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.SmallDateTime ) );
                Assert.That( sp.Parameters[7].Variable.TypeDecl.SyntaxSize, Is.EqualTo( -2 ), "Size does not apply." );

                Assert.That( sp.Parameters[8].IsOutput, Is.False );
                Assert.That( sp.Parameters[8].IsInputOutput, Is.False );
                Assert.That( sp.Parameters[8].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[8].DefaultValue.IsVariable, Is.False );
                Assert.That( sp.Parameters[8].DefaultValue.IsNull, Is.True );
                Assert.That( sp.Parameters[8].DefaultValue.IsLiteral, Is.False );

                Assert.That( sp.Header.ToStringCompact(), Is.EqualTo( "procedure CK.sStoredProcedureInputOutput @p1 int, @p2 tinyint=0, @p3 smallint output, @p4 nvarchar(50)=N'Murfn...', @p5 varchar(max) /*input*/output, @p6 char /*input*/output, @p7 Xml output, @p8 smalldatetime /*input*/output, @p9 smalldatetime=null" ) );
            } );
        }

        [DebuggerStepThrough]
        internal static T CheckStatement<T>( string fileName, Action<T> check ) where T : ISqlStatement
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
                                    result.LogOnError( TestHelper.ConsoleMonitor );
                                    TestHelper.ConsoleMonitor.Trace().Send( fullBody );
                                }
                                else TestHelper.ConsoleMonitor.Trace().Send( "Successfuly parsed: " + result.Result.ToStringSignature( true ) );
                            }
                            catch( Exception ex )
                            {
                                TestHelper.ConsoleMonitor.Fatal().Send( ex );
                            }
                        }
                    }
                }
            }

        }
    }
}
