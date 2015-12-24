using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CK.SqlServer.Parser
{
    public class SqlToXmlVisitor : SqlItemVisitor
    {
        readonly bool _combineElementType;
        XElement _current;

        public SqlToXmlVisitor( bool combineElementType = false )
        {
            _combineElementType = combineElementType;
        }

        public XElement ToXml( string name, ISqlNode item )
        {
            var prev = _current;
            var e = _current = new XElement( name );
            VisitItem( item );
            _current = prev;
            return e;
        }

        XElement StartNode( ISqlNode e )
        {
            string typeName = e.GetType().Name;
            if( typeName.StartsWith( "SqlToken" ) ) typeName = typeName.Substring( 8 );
            else if( typeName.StartsWith( "Sql" ) ) typeName = typeName.Substring( 3 );
            return StartNode( typeName );
        }

        XElement StartNode( string typeName )
        {
            if( !_combineElementType )
            {
                var e = new XElement( typeName );
                _current.Add( e );
                _current = e;
            }
            else _current.Add( new XAttribute( "Type", typeName ) );
            OnStartNode( _current );
            return _current;
        }

        void OnStartNode( XElement e )
        {
        }

        protected override ISqlNode VisitTokenStandard( SqlToken e )
        {
            StartNode( e ).Add( e.ToString() );
            return e;
        }

        protected override ISqlNode VisitStandard( ISqlNode e )
        {
            var props = e.GetType().GetProperties()
                                .Where( p => p.Name != "UnPar" )
                                .Where( p => p.Name != "StatementTerminator" )
                                .Where( p => typeof( ISqlNode ).IsAssignableFrom( p.PropertyType )
                                                && p.GetIndexParameters().Length == 0 )
                                .Select( p => new { Name = p.Name, Value = (ISqlNode)p.GetValue( e ) } )
                                .Where( o => o.Value != null );
            if( !props.Any() && e is IEnumerable<ISqlNode> )
            {
                props = ((IEnumerable<ISqlNode>)e).Select( x => new { Name = "Item", Value = x } );
            }
            StartNode( e )
                .Add( props.Select( o => ToXml( o.Name, o.Value ) ) );
            if( e is ISqlStatement && ((ISqlStatement)e).StatementTerminator != null )
            {
                _current.Add( new XAttribute( "HasStatementTerminator", "true" ) );
            }
            return e;
        }

        public override ISqlNode Visit( SqlEnclosedCommaList e )
        {
            StartNode( "CommaList" ).Add( e.Select( item => ToXml( "Item", item ) ) );
            if( e.IsEnclosed ) _current.Add( new XAttribute( "IsEnclosed", "true" ) );
            return e;
        }

        public override ISqlNode Visit( SqlTokenIdentifier e )
        {
            StartNode( "Identifier" ).Add( e.ToString() );
            if( e.IsVariable ) _current.Add( new XAttribute( "IsVariable", "true" ) );
            if( e.IsQuoted ) _current.Add( new XAttribute( "IsQuoted", "true" ) );
            if( e.IsDbType ) _current.Add( new XAttribute( "IsDbType", "true" ) );
            if( e.IsReservedKeyword ) _current.Add( new XAttribute( "IsReservedKeyword", "true" ) );
            return e;
        }

        public override ISqlNode Visit( SqlMultiIdentifier e )
        {
            StartNode( e ).Add( e.ToString() );
            if( e.IsVariable ) _current.Add( new XAttribute( "IsVariable", "true" ) );
            return e;
        }

        public override ISqlNode Visit( SqlTokenLiteralString e )
        {
            base.Visit( e );
            if( e.IsUnicode ) _current.Add( new XAttribute( "IsUnicode", "true" ) );
            return e;
        }

        public override ISqlNode Visit( SqlBetween e )
        {
            StartNode( "Between" ).Add(
                e.IsNotBetween ? new XAttribute( "IsNotBetween", "true" ) : null,
                ToXml( "Left", e.Left ),
                ToXml( "Start", e.Start ),
                ToXml( "Stop", e.Stop ) );
            return e;
        }

        public override ISqlNode Visit( SqlLike e )
        {
            StartNode( "Like" ).Add(
                e.IsNotLike ? new XAttribute( "IsNotLike", "true" ) : null,
                ToXml( "Left", e.Left ),
                ToXml( "Pattern", e.Pattern ) );
            return e;
        }

        public override ISqlNode Visit( SqlPar e )
        {
            StartNode( "Par" ).Add( ToXml( "Content", e.Content ) );
            return e;
        }

    }
}
