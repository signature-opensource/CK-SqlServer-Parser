using CK.Core;
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
        readonly IActivityMonitor _monitor;
        LocationRoot _root;

        public SqlNodeTransformer( ISqlNode node, IActivityMonitor monitor )
        {
            if( node == null ) throw new ArgumentNullException( nameof( node ) );
            if( monitor == null ) throw new ArgumentNullException( nameof( monitor ) );
            _root = new LocationRoot( node, false );
            _monitor = monitor;
        }

        /// <summary>
        /// Gets the current node. 
        /// This property tracks the transformed node.
        /// </summary>
        public ISqlNode Node => _root.Node;

        /// <summary>
        /// Gets the monitor used by this transformer.
        /// </summary>
        public IActivityMonitor Monitor => _monitor;

        /// <summary>
        /// Visits the root node with a location-aware visitor.
        /// If the visitor alters the structure, the <see cref="Node"/> is updated.
        /// </summary>
        /// <param name="transformer">A transformer visitor.</param>
        /// <param name="scope">An optional scope for the transformation.</param>
        public bool Apply( SqlNodeLocationVisitor transformer, SqlNodeScopeBuilder scope = null )
        {
            if( transformer == null ) throw new ArgumentNullException( nameof( transformer ) );
            ISqlNodeLocationRange filter = null;
            if( scope != null )
            {
                filter = BuildRange( scope );
                if( filter == null ) return false;
            }
            bool success = true;
            using( _monitor.OnError( () => success = false ) )
            {
                ISqlNode r = transformer.VisitRoot( _root, filter );
                if( r != _root.Node && success ) _root = new LocationRoot( r, false );
            }
            return success;
        }

        /// <summary>
        /// Visits the root node with a simple, non location-aware, visitor. No range filtering is supported.
        /// If the visitor alters the structure, the <see cref="Node"/> is updated.
        /// </summary>
        /// <param name="rawVisitor">A mere visitor.</param>
        public void Visit( SqlNodeVisitor rawVisitor )
        {
            ISqlNode r = rawVisitor.VisitRoot( _root.Node );
            if( r != _root.Node ) _root = new LocationRoot( r, false );
        }

        /// <summary>
        /// Visits the root node with a location-aware visitor.
        /// If the visitor alters the structure, the <see cref="Node"/> is updated.
        /// </summary>
        /// <param name="visitor">A visitor.</param>
        /// <param name="rangeFilter">An optional filter that restricts the visit.</param>
        public void Visit( SqlNodeLocationVisitor visitor, ISqlNodeLocationRange rangeFilter = null )
        {
            if( visitor == null ) throw new ArgumentNullException( nameof( visitor ) );
            ISqlNode r = visitor.VisitRoot( _root, rangeFilter );
            if( r != _root.Node ) _root = new LocationRoot( r, false );
        }

        class ScopeResolver : SqlNodeLocationVisitor
        {
            readonly SqlNodeScopeBuilder _builder;
            readonly List<SqlNodeLocationRange> _ranges;

            public ScopeResolver( SqlNodeScopeBuilder builder, IActivityMonitor m )
            {
                Monitor = m;
                builder.Reset();
                _builder = builder;
                _ranges = new List<SqlNodeLocationRange>();
            }

            public ISqlNodeLocationRange Result => SqlNodeLocationRange.Create( _ranges, _ranges.Count, false );

            protected override ISqlNode VisitStandard( ISqlNode e ) => VisitStandardReadOnly( e );

            protected override bool BeforeVisitItem()
            {
                ISqlNodeLocationRange r = _builder.Enter( VisitContext );
                if( r != null )
                {
                   _ranges.AddRange( r );
                    if( r.Last.End.Position >= VisitContext.Position + VisitContext.VisitedNode.Width ) return false;
                }
                return true;
            }

            protected override ISqlNode AfterVisitItem( ISqlNode visitResult )
            {
                ISqlNodeLocationRange r = _builder.Leave( VisitContext );
                if( r != null ) _ranges.AddRange( r );
                if( VisitContext.Depth == 0 )
                {
                    r = _builder.Conclude( VisitContext );
                    if( r != null ) _ranges.AddRange( r );
                }
                return visitResult;
            }
        }

        /// <summary>
        /// Applies a <see cref="SqlNodeScopeBuilder"/> to the current <see cref="Node"/> root.
        /// </summary>
        /// <param name="builder">The scope builder.</param>
        /// <param name="rangeFilter">An optional filter that restricts the visit.</param>
        /// <returns>A result range or null on error.</returns>
        public ISqlNodeLocationRange BuildRange( SqlNodeScopeBuilder builder, ISqlNodeLocationRange rangeFilter = null )
        {
            if( builder == null ) throw new ArgumentNullException( nameof( builder ) );
            bool error = false;
            using( _monitor.OnError( () => error = true ) )
            {
                var s = new ScopeResolver( builder, _monitor );
                s.VisitRoot( _root, rangeFilter );
                return error ? null : s.Result;
            }
        }


    }
}
