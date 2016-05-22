using CK.Core;
using CK.SqlServer.Parser;
using CK.SqlServer.UtilTests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.SqlServer.Transform.Transformers;

namespace CK.SqlServer.Transform.Tests.XmlTests
{

    [TestFixture]
    public class XmlTests
    {
        class XmlSqlTesterWithTransform : XmlSqlTester
        {
            public readonly Func<ISqlNode,ISqlNode> Transformer;
            public readonly string ResultText;

            public XmlSqlTesterWithTransform( XElement e )
                : base( e )
            {
                if( Description != null )
                {
                    if( Description.StartsWith( "CALL: " ) )
                    {
                        var method = typeof( XmlTests ).GetMethod( Description.Substring( 6 ).Trim() );
                        Transformer = (Func<ISqlNode, ISqlNode>)method.CreateDelegate( typeof( Func<ISqlNode, ISqlNode> ) );
                    }
                    else
                    {
                        SqlTransformer t = ParseTransformer( Description );
                        Transformer = n =>
                        {
                            SqlNodeTransformer host = new SqlNodeTransformer( n, TestHelper.ConsoleMonitor );
                            Assert.That( host.Apply( t ) );
                            return host.Node;
                        };
                    }
                }
                ResultText = ((string)TestElement.Element( "ResultText" ))?.TrimEnd().NormalizeEOL();
            }

            protected override ISqlNode OnParsed( ISqlNode e )
            {
                if( Transformer != null ) e = Transformer( e );
                if( ResultText != null )
                {
                    string actualText = e.ToString( true, true );
                    using( TestHelper.ConsoleMonitor.OpenInfo().Send( "Expected Result" ) )
                    {
                        TestHelper.ConsoleMonitor.Trace().Send( ResultText );
                    }
                    using( TestHelper.ConsoleMonitor.OpenInfo().Send( "Actual Result" ) )
                    {
                        TestHelper.ConsoleMonitor.Trace().Send( actualText );
                    }

                    ISqlNode resultNode = ParseAndCheckSqlText( ResultText );
                    string actual = e.ToStringHyperCompact();
                    string expected = resultNode.ToStringHyperCompact();
                    if( actual != expected )
                    {
                        Assert.That( actual, Is.EqualTo( expected ) );
                    }
                    if( actualText != ResultText ) TestHelper.ConsoleMonitor.Warn().Send( "Rendering is not perfect..." );
                }
                return base.OnParsed( e );
            }

            static SqlTransformer ParseTransformer( string text )
            {
                var r = new SqlServerParser().Parse( text );
                Assert.That( r.IsError, Is.False, r.ErrorMessage );
                Assert.That( r.Result, Is.Not.Null );
                Assert.That( r.Result, Is.InstanceOf<SqlTransformer>() );
                return (SqlTransformer)r.Result;
            }
        }
        

        [TestCase( "CK.DB.Basics.xml" )]
        [TestCase( "Insert location selector.xml" )]
        public void file_test( string fileName )
        {
            XmlSqlTester.RunAllTests( fileName, e => new XmlSqlTesterWithTransform( e ) );
        }

        public static ISqlNode GroupCreateToZone( ISqlNode e )
        {
            SqlNodeTransformer t = new SqlNodeTransformer( e, TestHelper.ConsoleMonitor );
            SqlAnalyser a = new SqlAnalyser( "@ZoneId int = 0" );

            SqlParameter pZoneId = a.IsParameter( true );
            t.Visit( new AddParameter( new[] { pZoneId }, null, "@GroupIdResult" ) );

            ISqlNodeLocationRange ifStatements = t.BuildRange( new SqlNodeScopePredicate( n => n is SqlIf ) );
            SqlNodeLocation headLoc = ifStatements.First.End;
            a.Reset( "if @ZoneId = 1 throw 50000, 'Zone.SystemZoneHasNoGroup', 1;" );
            var newGuard = (ISqlStatement)a.Parse( ParseMode.Statement );
            t.Visit( new InsertStatement( headLoc, newGuard ) );

            t.Visit( new AddColumnInInsert( SqlTokenIdentifier.Create( "ZoneId" ), pZoneId.Variable.Identifier ) );

            return t.Node;
        }

    }
}
