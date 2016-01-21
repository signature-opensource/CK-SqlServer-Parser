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
            SqlAnalyser.ErrorResult r = SqlAnalyser.Parse( out e, ParseMode.AllStatements, text );
            Assert.That( r.IsError, Is.False, r.ToString() );
            Assert.That( e.ToString( true, true ).NormalizeEOL(), Is.EqualTo( text ) );
        }

        [Test]
        public void The_sp_GetDDL_script_is_correctlty_parsed()
        {
            string text = TestHelper.LoadTextFromParsingScripts( "sp_GetDDL.sql" );
            ISqlNode e;
            SqlAnalyser.ErrorResult r = SqlAnalyser.Parse( out e, ParseMode.AllStatements, text );
            Assert.That( r.IsError, Is.False, r.ToString() );
            Assert.That( e.ToString( true, true ).NormalizeEOL(), Is.EqualTo( text ) );

            XElement visited = new SqlToXmlStatementVisitor().ToXml( "Statements", e );
            string visitedString = visited.ToString();
            TestHelper.ConsoleMonitor.Trace().Send( visitedString );

            Assert.That( ((SqlNodeList)e).Count, Is.EqualTo( 7 ) );
        }

        [Test]
        public void checking_different_kind_of_parameters()
        {
            CheckStatement<SqlStoredProcedure>( "sStoredProcedureInputOutput.sql", sp =>
            {
                Assert.That( sp.Name.Identifiers[0].ToString(), Is.EqualTo( "CK" ) );
                Assert.That( sp.Name.Identifiers[1].ToString(), Is.EqualTo( "sStoredProcedureInputOutput" ) );
                Assert.That( sp.Name.ToString(), Is.EqualTo( "CK.sStoredProcedureInputOutput" ) );

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

                Assert.That( sp.Header.ToStringCompact(), Is.EqualTo( "procedure CK.sStoredProcedureInputOutput @p1 int, @p2 tinyint = 0, @p3 smallint output, @p4 nvarchar(50)=N'Murfn...', @p5 varchar(max) /*input*/output, @p6 char /*input*/output, @p7 Xml output, @p8 smalldatetime /*input*/output, @p9 smalldatetime = null" ) );
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

    }
}
