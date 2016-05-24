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
        /// Base context exposes the <see cref="LocationManager"/> and <see cref="AddError"/> method
        /// and is avalable to .
        /// </summary>
        public interface IVisitContextBase
        {
            /// <summary>
            /// Gets the location manager to use.
            /// </summary>
            ISqlNodeLocationManager LocationManager { get; }

            /// <summary>
            /// Gets the monitor to use to raise error or to say something to the external world.
            /// </summary>
            IActivityMonitor Monitor { get; }

            /// <summary>
            /// Gets the current range filter. Can be null.
            /// </summary>
            ISqlNodeLocationRange RangeFilter { get; }
        }

        /// <summary>
        /// This context is available on <see cref="VisitContext"/> property.
        /// </summary>
        public interface IVisitContext : IVisitContextBase
        {
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
            ISqlNodeLocationRange _rangeFilter;
            IEnumerator<SqlNodeLocationRange> _filteredRange;
            IActivityMonitor _monitor;
            bool _inScope;

            public ISqlNodeLocationRange RangeFilter => _rangeFilter;

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

            public IActivityMonitor Monitor
            {
                get { return _monitor; }
                set { _monitor = value; }
            }


            public void Reset( LocationRoot root, ISqlNodeLocationRange rangeFilter )
            {
                _builder.Reset( root );
                Debug.Assert( _builder.Depth == -1 );
                _filteredRange = null;
                if( (_rangeFilter = rangeFilter) != null )
                {
                    var e = rangeFilter.GetEnumerator();
                    if( e.MoveNext() ) _filteredRange = e;
                }
                _inScope = true;
            }

            public void EnsureRootForNode( ISqlNode root )
            {
                if( root != _builder.Root?.Node ) _builder.Reset( new LocationRoot( root ) );
            }

            public bool Enter( ISqlNode prev, ISqlNode n )
            {
                VisitedNode = n;
                _builder.Enter( n );
                if( !_inScope )
                {
                    Leave( prev, true );
                    return false;
                }
                return true;
            }

            public void Leave( ISqlNode prev, bool skipped )
            {
                _builder.Leave( VisitedNode, skipped );
                VisitedNode = prev;
                if( _filteredRange != null )
                {
                    int p = _builder.Position;
                    while( p < _filteredRange.Current.Beg.Position )
                    {
                        if( !_filteredRange.MoveNext() )
                        {
                            _inScope = false;
                            return;
                        }
                    }
                    _inScope = p < _filteredRange.Current.End.Position;
                }
            }

            public ISqlNodeLocationManager LocationManager => _builder.Root;

            public ISqlNode VisitedNode { get; private set; }

            public LocationRoot Root => _builder.Root;

            public int Depth => _builder.Depth;

            public int Position => _builder.Position;

            public SqlNodeLocation GetCurrentLocation( bool ensureQualifiedLocation )
            {
                return VisitedNode != null ? _builder.GetCurrent( VisitedNode, ensureQualifiedLocation ) : _builder.Root;
            }

        }

        readonly VContext _context;
        bool _hasUnParsedText;
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
        /// Gets or sets the monitor.
        /// </summary>
        public IActivityMonitor Monitor
        {
            get { return _context.Monitor; }
            set { _context.Monitor = value; }
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
            _context.EnsureRootForNode( root );
            return VisitRoot( _context.Root, null );
        }

        internal ISqlNode VisitRoot( LocationRoot root, ISqlNodeLocationRange rangeFilter )
        {
            Debug.Assert( root != null && root.Node != null );
            _context.Reset( root, rangeFilter );
            return base.VisitRoot( root.Node );
        }

        /// <summary>
        /// Overridden to use <see cref="VisitStandard"/> otherwise type declaration would be skipped by the visit.
        /// </summary>
        /// <param name="e">The type declaration to process.</param>
        /// <returns>Result of the visit.</returns>
        protected override ISqlNode VisitTypeDeclStandard( ISqlUnifiedTypeDecl e ) => VisitStandard( e );

        /// <summary>
        /// Overridden to update <see cref="VisitContext"/> and check scope. If the node is in the scope,
        /// calls <see cref="BeforeVisitItem"/>, call the visit itself (base method), call <see cref="AfterVisitItem"/> 
        /// and restore VisitContext.
        /// </summary>
        /// <param name="e">The node to visit.</param>
        /// <returns>The visited result node.</returns>
        protected override ISqlNode VisitItem( ISqlNode e )
        {
            ISqlNode v = e;
            var prev = _context.VisitedNode;
            if( _context.Enter( prev, e ) )
            {
                bool doChildrenVisit = BeforeVisitItem() && !_stop;
                if( doChildrenVisit ) v = base.VisitItem( e );
                v = AfterVisitItem( v );
                _context.Leave( prev, !doChildrenVisit );
            }
            return v;
        }

        /// <summary>
        /// Gets whether unparsed text has been injected during the transformation.
        /// </summary>
        public bool HasUnParsedText => _hasUnParsedText;

        /// <summary>
        /// Called by <see cref="VisitItem"/> before the visit. 
        /// The <see cref="VisitContext"/> is bound to the node that will be visited.
        /// </summary>
        /// <param name="ctx">The current context visit.</param>
        /// <returns>
        /// True (the default) to visit the children. False to skip the visit of the current node. 
        /// </returns>
        protected virtual bool BeforeVisitItem() => true;

        /// <summary>
        /// Called by <see cref="VisitItem(ISqlNode)"/> after the visit.
        /// The <see cref="VisitContext"/> is bound to the node that has been visited.
        /// </summary>
        /// <param name="visitResult">
        /// The visited node (same as <see cref="VisitContext"/>.VisitedNode if no mutation occurred).
        /// </param>
        /// <returns>The visitResult node.</returns>
        protected virtual ISqlNode AfterVisitItem( ISqlNode visitResult ) => visitResult;

        /// <summary>
        /// Calling this method stops the visit.
        /// </summary>
        /// <param name="hasUnParsedText">Optionally sets <see cref="HasUnParsedText"/> to true.</param>
        protected void StopVisit( bool hasUnParsedText = false )
        {
            _hasUnParsedText |= hasUnParsedText;
            _stop = true;
        }

        /// <summary>
        /// Sets <see cref="HasUnParsedText"/> to true.
        /// </summary>
        protected void SetHasUnParsedText() => _hasUnParsedText = true;

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
