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
using CK.Text;
using System.Reflection;

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
                            SqlTransformHost host = new SqlTransformHost( n, TestHelper.ConsoleMonitor );
                            return host.Apply( t )  ? host.Node : null;
                        };
                    }
                }
                ResultText = ((string)TestElement.Element( "ResultText" ))?.TrimEnd().NormalizeEOL();
            }

            protected override ISqlNode OnParsed( ISqlNode e )
            {
                IReadOnlyList<ActivityMonitorSimpleCollector.Entry> errors = null;
                using( TestHelper.ConsoleMonitor.CollectEntries( err => errors = err ) )
                {
                    if( Transformer != null ) e = Transformer( e );
                }
                if( ResultText != null )
                {
                    if( ResultText.StartsWith( "ERROR:" ) )
                    {
                        string errContains = ResultText.Substring( 6 ).Trim();
                        Assert.That( errors != null, $"Error expected '{errContains}'." );
                        var all = errors.Select( err => err.Text ).Concatenate( Environment.NewLine );
                        Assert.That( all, Does.Contain( errContains ), "Expected error not found." );
                        return null;
                    }
                    string actualText = e.ToString( true, true );
                    using( TestHelper.ConsoleMonitor.OpenInfo( "Expected Result" ) )
                    {
                        TestHelper.ConsoleMonitor.Trace( ResultText );
                    }
                    using( TestHelper.ConsoleMonitor.OpenInfo( "Actual Result" ) )
                    {
                        TestHelper.ConsoleMonitor.Trace( actualText );
                    }

                    ISqlNode resultNode = ParseAndCheckSqlText( ResultText, ResultText );
                    string actual = e.ToStringHyperCompact();
                    string expected = resultNode.ToStringHyperCompact();
                    if( actual != expected )
                    {
                        Assert.That( actual, Is.EqualTo( expected ) );
                    }
                    if( actualText != ResultText ) TestHelper.ConsoleMonitor.Warn( "Rendering is not perfect..." );
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


        [TestCase( "_Current.xml" )]
        [TestCase( "Add column.xml" )]
        [TestCase( "CK.DB.Basics.xml" )]
        //[TestCase( "Combine Select.xml" )]
        [TestCase( "Each support.xml" )]
        [TestCase( "General.xml" )]
        [TestCase( "In scope.xml" )]
        [TestCase( "Inject around.xml" )]
        [TestCase( "Inject location range.xml" )]
        [TestCase( "Inject location.xml" )]
        [TestCase( "Replace with.xml" )]
        [TestCase( "Stored Procedures.xml" )]
        [TestCase( "Transformer scope.xml" )]
        public void file_test( string fileName )
        {
            XmlSqlTester.RunAllTests( fileName, e => new XmlSqlTesterWithTransform( e ) );
        }

        public static ISqlNode GroupCreateToZone( ISqlNode e )
        {
            SqlTransformHost t = new SqlTransformHost( e, TestHelper.ConsoleMonitor );
            SqlAnalyser a = new SqlAnalyser( "@ZoneId int = 0" );

            SqlParameter pZoneId = a.IsParameter( true );
            t.Visit( new AddParameter( new[] { pZoneId }, null, "@GroupIdResult" ) );

            ISqlNodeLocationRange ifStatements = t.BuildRange( new SqlNodeScopeBreadthPredicate( n => n is SqlIf ) );
            SqlNodeLocation headLoc = ifStatements.First.End;
            a.Reset( "if @ZoneId = 1 throw 50000, 'Zone.SystemZoneHasNoGroup', 1;" );
            var newGuard = (ISqlStatement)a.Parse( ParseMode.Statement );
            t.Visit( new InsertStatement( headLoc, newGuard ) );

            var newC = new SelectColumn( SqlTokenIdentifier.Create( "ZoneId" ), SqlKeyword.Assign, pZoneId.Variable.Identifier );
            t.Visit( new AddColumn( new[] { newC } ) );

            return t.Node;
        }

    }
}
