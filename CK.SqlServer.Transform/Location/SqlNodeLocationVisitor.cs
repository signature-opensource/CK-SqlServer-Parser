using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{

    /// <summary>
    /// Extends <see cref="SqlNodeVisitor"/> to support <see cref="SqlNodeLocation"/> handling
    /// and <see cref="StopVisit"/> capability.
    /// </summary>
    public class SqlNodeLocationVisitor : SqlNodeVisitor
    {
        /// <summary>
        /// This context is available on <see cref="VisitContext"/> property.
        /// </summary>
        public interface IVisitContext
        {
            /// <summary>
            /// Gets the location manager to use.
            /// </summary>
            ISqlNodeLocationManager LocationManager { get; }

            /// <summary>
            /// Gets the visited node.
            /// </summary>
            ISqlNode VisitedNode { get; }

            /// <summary>
            /// Gets the current depth.
            /// </summary>
            int Depth { get; }

            /// <summary>
            /// Gets the current position.
            /// </summary>
            int Position { get; }

            /// <summary>
            /// Obtains the location of the currently visited node.
            /// When no nodes are beeing visited, the root is returned.
            /// </summary>
            /// <param name="qualifiedLocation">True to force the obtention of a qualified location.</param>
            /// <returns>A (potentially qualified) location.</returns>
            SqlNodeLocation GetCurrentLocation( bool ensureQualifiedLocation = false );
        }

        class VContext : IVisitContext
        {
            ISqlNodeLocationBuilder _builder;

            public bool BuildQualifiedNodeLocations
            {
                get { return _builder is QualifiedLocationBuilder; }
                set
                {
                    if( _builder == null || value != _builder is QualifiedLocationBuilder )
                    {
                        _builder = value ? (ISqlNodeLocationBuilder)new QualifiedLocationBuilder() : new LightLocationBuilder();
                    }
                }
            }

            public void Reset( LocationRoot root )
            {
                _builder.Reset( root );
                Debug.Assert( _builder.Depth == -1 );
            }

            public void Reset( ISqlNode root )
            {
                if( root != _builder.Root?.Node ) Reset( new LocationRoot( root ) );
            }

            public void Enter( ISqlNode n )
            {
                VisitedNode = n;
                _builder.Enter( n );
            }

            public void Leave( ISqlNode prev )
            {
                _builder.Leave( VisitedNode );
                VisitedNode = prev;
            }

            public ISqlNodeLocationManager LocationManager => _builder.Root;

            public ISqlNode VisitedNode { get; private set; }

            public LocationRoot Root => _builder.Root;

            public int Depth => _builder.Depth;

            public int Position => _builder.Position;

            public SqlNodeLocation GetCurrentLocation( bool ensureQualifiedLocation ) => _builder.GetCurrent( VisitedNode, ensureQualifiedLocation );
        }

        readonly VContext _context;
        bool _stop;

        /// <summary>
        /// Initializes a new location visitor.
        /// </summary>
        /// <param name="buildQualifiedNodeLocations">True to build qualified locations by default instead of raw ones.</param>
        protected SqlNodeLocationVisitor( bool buildQualifiedNodeLocations = false )
        {
            _context = new VContext() { BuildQualifiedNodeLocations = buildQualifiedNodeLocations };
        }

        /// <summary>
        /// Gets or sets whether qualified locations must be built by default or raw ones are enough.
        /// </summary>
        public bool BuildQualifiedNodeLocations
        {
            get { return _context.BuildQualifiedNodeLocations; }
            set { _context.BuildQualifiedNodeLocations = value; }
        }

        /// <summary>
        /// Overridden to adapt this public method to the internals of this implementation.
        /// It is not intented to be used directly.
        /// </summary>
        /// <param name="root">The root node to vissit.</param>
        /// <returns>The visited result.</returns>
        public override sealed ISqlNode VisitRoot( ISqlNode root )
        {
            if( root == null ) throw new ArgumentNullException( nameof( root ) );
            _context.Reset( root );
            return VisitRoot( _context.Root );
        }

        internal ISqlNode VisitRoot( LocationRoot root )
        {
            Debug.Assert( root != null && root.Node != null );
            _context.Reset( root );
            return base.VisitRoot( root.Node );
        }

        /// <summary>
        /// Overridden to update <see cref="VisitContext"/>, call <see cref="BeforeVisitItem"/>, 
        /// call the visit itself (base method), call <see cref="AfterVisitItem"/> and restore VisitContext.
        /// </summary>
        /// <param name="e">The node to visit.</param>
        /// <returns>The visited result node.</returns>
        protected override ISqlNode VisitItem( ISqlNode e )
        {
            ISqlNode v = e;
            if( e.Width != 0 )
            {
                var prev = _context.VisitedNode;
                _context.Enter( e );
                if( BeforeVisitItem() )
                {
                    if( !_stop ) v = base.VisitItem( e );
                    v = AfterVisitItem( v );
                }
                _context.Leave( prev );
            }
            return v;
        }

        /// <summary>
        /// Called by <see cref="VisitItem"/> before the visit. 
        /// The <see cref="VisitContext"/> is bound to the node that will be visited.
        /// </summary>
        /// <param name="ctx">The current context visit.</param>
        /// <returns>
        /// False to skip the visit of the current node (and the call to <see cref="AfterVisitItem(ISqlNode)"/>).
        /// False to visit the children.
        /// </returns>
        protected virtual bool BeforeVisitItem()
        {
            return true;
        }

        /// <summary>
        /// Called by <see cref="VisitItem(ISqlNode)"/> after the visit.
        /// The <see cref="VisitContext"/> is bound to the node that has been visited.
        /// </summary>
        /// <param name="visitResult">
        /// The visited node (same as <see cref="VisitContext"/>.VisitedNode if no mutation occurred).
        /// </param>
        /// <returns>The visitResult node.</returns>
        protected virtual ISqlNode AfterVisitItem( ISqlNode visitResult )
        {
            return visitResult;
        }

        /// <summary>
        /// Calling this method stops the visit.
        /// </summary>
        protected void StopVisit() => _stop = true;

        /// <summary>
        /// Gets whether <see cref="StopVisit"/> has been called.
        /// </summary>
        protected bool IsStoppedVisit => _stop;

        /// <summary>
        /// Gets the current visit context. 
        /// </summary>
        protected IVisitContext VisitContext => _context;

    }
}
