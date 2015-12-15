using CK.Core;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace CK.SqlServer.Parser.Tests
{

    /// <summary>
    /// Offers utility methods to deal with Sql Server objects and data.
    /// Borrowed from CK.SqlServer.Core.
    /// </summary>
    public class SqlHelper
    {
        static readonly Type[] _typesMap = new Type[] 
        {
            typeof(Int64), // SqlDbType.BigInt
            typeof(byte[]), // SqlDbType.Binary
            typeof(bool), // SqlDbType.Bit
            typeof(string), // SqlDbType.Char
            typeof(DateTime), // SqlDbType.DateTime
            typeof(Decimal), // SqlDbType.Decimal
            typeof(Double), // SqlDbType.Float
            typeof(byte[]), // SqlDbType.Image
            typeof(Int32), // SqlDbType.Int
            typeof(Decimal), // SqlDbType.Money
            typeof(string), // SqlDbType.NChar
            typeof(string), // SqlDbType.NText
            typeof(string), // SqlDbType.NVarChar
            typeof(Single), // SqlDbType.Real
            typeof(Guid), // SqlDbType.UniqueIdentifier
            typeof(DateTime), // SqlDbType.SmallDateTime
            typeof(Int16), // SqlDbType.SmallInt
            typeof(Decimal), // SqlDbType.SmallMoney
            typeof(string), // SqlDbType.Text
            typeof(byte[]), // SqlDbType.Timestamp
            typeof(byte), // SqlDbType.TinyInt
            typeof(byte[]), // SqlDbType.VarBinary
            typeof(string), // SqlDbType.VarChar
            typeof(object), // SqlDbType.Variant
            null,
            typeof(string), // SqlDbType.Xml
            null, null, null,
            typeof(object), // SqlDbType.Udt
            typeof(object), // SqlDbType.Structured
            typeof(DateTime), // SqlDbType.Date
            typeof(DateTime), // SqlDbType.Time
            typeof(DateTime), // SqlDbType.DateTime2
            typeof(DateTimeOffset), // SqlDbType.DateTimeOffset
        };

        /// <summary>
        /// Simple association to a Type from a Sql type.
        /// </summary>
        /// <param name="tSql">Sql type.</param>
        /// <returns>.net type to consider.</returns>
        static public Type FromSqlDbTypeToNetType( SqlDbType tSql )
        {
            Debug.Assert( _typesMap.Length == 35 );
            return _typesMap[(int)tSql];
        }

    }
    /// <summary>
    /// Borrowed from CK.SqlServer.Setup.Runtime.
    /// </summary>
    public static class ISqlServerExtensions
    {
        static public Type BestNetType( this ISqlServerUnifiedTypeDecl @this )
        {
            SqlDbType sql = @this.DbType;
            if( sql == SqlDbType.Char || sql == SqlDbType.NChar )
            {
                int sz = @this.SyntaxSize;
                if( sz == 0 || sz == 1 )
                {
                    return typeof( char );
                }
                return typeof( string );
            }
            return SqlHelper.FromSqlDbTypeToNetType( sql );
        }
    }

    class CSharper : SqlItemVisitor
    {
        TextWriter _sw;
        IndentedTextWriter _w;

        public CSharper()
        {
            _sw = new StringWriter();
            _w = new IndentedTextWriter( _sw, "  " );
        }

        public override SqlNode Visit( SqlExprStIf e )
        {
            _w.Write( "if( " );
            VisitItem( e.Condition );
            _w.WriteLine( " )" );
            _w.WriteLine( "{" );
            _w.Indent += 1;
            VisitItem( e.ThenStatement );
            _w.Indent -= 1;
            _w.WriteLine( "}" );
            if( e.HasElse )
            {
                _w.WriteLine( "else" );
                _w.WriteLine( "{" );
                _w.Indent += 1;
                VisitItem( e.ElseStatement );
                _w.Indent -= 1;
                _w.WriteLine( "}" );
            }
            return e;
        }

        public override SqlNode Visit( SqlExprUnaryOperator e )
        {
            _w.Write( e.OperatorT.ToString() );
            VisitItem( e.Expression );
            return e;
        }

        public override SqlNode Visit( SqlExprKoCall e )
        {
            _w.Write( e.FunName );
            _w.Write( "(" );
            int count = 0;
            foreach( var p in e.Parameters )
            {
                _w.Write( count > 0 ? ", " : " " );
                VisitItem( p );
                ++count;
            }
            _w.Write( count > 0 ? " )" : ")" );
            return e;
        }

        public override SqlNode Visit( SqlExprBinaryOperator e )
        {
            for( int i = 0; i < e.Opener.Tokens.Count; ++i ) _w.Write( "(" );
            VisitItem( e.Left );
            _w.Write( " " );
            string op;
            switch( e.Operator.TokenType )
            {
                case SqlTokenType.Equal: op = "=="; break;
                case SqlTokenType.And: op = "&&"; break;
                case SqlTokenType.Or: op = "||"; break;
                case SqlTokenType.NotEqualTo: op = "!="; break;
                default: op = e.Operator.ToString(); break;
            }
            _w.Write( op );
            _w.Write( " " );
            VisitItem( e.Right );
            if( e.Closer.Tokens.Count > 0 ) _w.Write( ")" );
            return e;
        }

        public override SqlNode Visit( SqlExprDeclare e )
        {
            Type t = e.Variable.TypeDecl.ActualType.BestNetType();
            if( t != null ) _w.Write( t.Name );
            else
            {
                _w.Write( "/* Unsuported: " );
                _w.Write( e.Variable.ToString() );
                _w.WriteLine( "*/" );
            }
            _w.Write( " " );
            _w.Write( MapVariableName( e.Variable.Identifier.Name ) );
            _w.WriteLine( ";" );
            return e;
        }

        public override SqlNode Visit( SqlExprBetween e )
        {
            _w.Write( "(" );
            VisitItem( e.Left );
            _w.Write( " >= " );
            VisitItem( e.Start );
            _w.Write( " && " );
            VisitItem( e.Left );
            _w.Write( " <= " );
            VisitItem( e.Stop );
            _w.Write( ")" );
            return e;
        }

        public override SqlNode Visit( SqlExprStLabelDef e )
        {
            _w.WriteLine( MapLabelName( e.IdentifierT.Name ) + ':' );
            return e;
        }

        public override SqlNode Visit( SqlExprStGoto e )
        {
            _w.Write( "goto " );
            _w.Write( MapLabelName( e.Target.Name ) );
            _w.WriteLine( ';' );
            return e;
        }

        public override SqlNode Visit( SqlExprStSetVar e )
        {
            _w.Write( MapVariableName( e.Variable.Name ) );
            _w.Write( " = " );
            VisitItem( e.Value );
            _w.WriteLine( ";" );
            return e;
        }

        public override SqlNode Visit( SqlExprIdentifier e )
        {
            if( e.IsVariable ) _w.Write( MapVariableName( e.Name ) );
            return e;
        }

        string MapVariableName( string v )
        {
            Debug.Assert( v[0] == '@' );
            return StdMapName( v.Substring( 1 ) );
        }

        string MapLabelName( string label )
        {
            return StdMapName( label );
        }

        static string StdMapName( string v )
        {
            v = v.Replace( "$", "_" );
            return v;
        }

        public override SqlNode Visit( SqlExprLiteral e )
        {
            _w.Write( e.Token.LiteralValue );
            return e;
        }

        public override SqlNode Visit( SelectSpecification e )
        {
            _w.Indent += 2;
            _w.WriteLine( "/* Select:" );
            _w.WriteLine( e.ToString() );
            _w.Indent -= 2;
            _w.WriteLine( "*/" );
            return e;
        }

        public override string ToString()
        {
            return _sw.ToString();
        }

    }


    [TestFixture]
    public class RewritingTests
    {
        [TestCase( "CLASSEMENT.sql" )]
        [TestCase( "CLASSEMENT_POSTE.sql" )]
        public void parsing_big_sp( string fileName )
        {
            SqlExprStStoredProc sp = SqlAnalyserTest.ReadStatement<SqlExprStStoredProc>( fileName );
            TestHelper.ConsoleMonitor.Trace().Send( sp.ToString() );
            var v = new CSharper();
            v.VisitItem( sp );
            TestHelper.ConsoleMonitor.Trace().Send( v.ToString() );
        }

    }



}
