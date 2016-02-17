using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    public class SqlNodeTransformer
    {
        LocationRoot _root;

        public SqlNodeTransformer( ISqlNode node )
        {
            if( node == null ) throw new ArgumentNullException( nameof( node ) );
            _root = new LocationRoot( node, false );
        }

        /// <summary>
        /// Gets the current node. 
        /// This property tracks the transformed node.
        /// </summary>
        public ISqlNode Node => _root.Node;

        public void Visit( SqlNodeVisitor rawVisitor )
        {
            ISqlNode r = rawVisitor.VisitRoot( _root.Node );
            if( r != _root.Node ) _root = new LocationRoot( r, false );
        }

        class ScopeResolver : SqlNodeLocationVisitor
        {
            readonly SqlNodeScopeBuilder _builder;
            readonly List<SqlNodeLocationRange> _ranges;

            public ScopeResolver( SqlNodeScopeBuilder builder )
            {
                builder.Reset();
                _builder = builder;
                _ranges = new List<SqlNodeLocationRange>();
            }

            public ISqlNodeLocationRange Result => _ranges.Count == 0 ? SqlNodeLocationRange.Empty : new LocationRangeList( _ranges );

            protected override ISqlNode VisitStandard( ISqlNode e ) => VisitStandardReadOnly( e );

            protected override void BeforeVisitItem()
            {
                ISqlNodeLocationRange r = _builder.Enter( VisitContext );
                if( r != null ) _ranges.AddRange( r );
            }

            protected override ISqlNode AfterVisitItem( ISqlNode visitResult )
            {
                ISqlNodeLocationRange r = _builder.Leave( VisitContext );
                if( r != null ) _ranges.AddRange( r );
                if( VisitContext.Depth == 0 )
                {
                    r = _builder.Conclude( VisitContext.LocationManager );
                    if( r != null ) _ranges.AddRange( r );
                }
                return visitResult;
            }
        }

        public ISqlNodeLocationRange BuildRange( SqlNodeScopeBuilder builder )
        {
            if( builder == null ) throw new ArgumentNullException( nameof( builder ) );
            var s = new ScopeResolver( builder );
            s.VisitRoot( _root );
            return s.Result;
        }


    }
}
