using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CK.SqlServer.Parser
{
    public class SqlToXmlStatementVisitor : SqlItemVisitor
    {
        XElement _current;

        public XElement ToXml( string name, ISqlNode item )
        {
            _current = new XElement( name );
            VisitItem( item );
            return _current;
        }

        protected override ISqlNode VisitStandard( ISqlNode e )
        {
            ISqlStatement st = e as ISqlStatement;
            if( st == null ) return base.VisitStandard( e );

            using( StartNode( e ) )
            {
                ISqlNamedStatement n = e as ISqlNamedStatement;
                if( n != null )
                {
                    string sn = n.GetStatementName();
                    if( sn != _current.Name && sn + "Statement" != _current.Name )
                    {
                        _current.Add( new XAttribute( "StatementName", sn ) );
                    }
                }
                if( st.StatementTerminator != null )
                {
                    _current.Add( new XAttribute( "HasTerminator", "true" ) );
                }
                base.VisitStandard( e );
            }
            return e;
        }

        IDisposable StartNode( ISqlNode e )
        {
            string typeName = e.GetType().Name;
            if( typeName.StartsWith( "SqlToken" ) ) typeName = typeName.Substring( 8 );
            else if( typeName.StartsWith( "Sql" ) ) typeName = typeName.Substring( 3 );
            var c = new XElement( typeName );
            var prev = _current;
            _current.Add( c );
            _current = c;
            return Util.CreateDisposableAction( () => { _current = prev; } );
        }

        public override ISqlNode Visit( SelectSpec e )
        {
            using( StartNode( e ) )
            {
                return base.Visit( e );
            }
        }

        public override ISqlNode Visit( SelectCombine e )
        {
            using( StartNode( e ) )
            {
                _current.Add( new XAttribute( "CombinationKind", e.SelectOperator.ToString() ) );
                return base.Visit( e );
            }
        }

        public override ISqlNode Visit( SelectDecorator e )
        {
            using( StartNode( e ) )
            {
                return base.Visit( e );
            }
        }
    }
}
