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
        class VContext : IVisitContext
        {
            readonly QualifiedLocationBuilder _builder;
            ISqlNodeLocationRange _rangeFilter;
            IEnumerator<SqlNodeLocationRange> _filteredRange;
            int _overridePos;
            VisitedNodeRangeFilterStatus _rangeFilterStatus;

            public VContext()
            {
                _builder = new QualifiedLocationBuilder();
            }

            public IActivityMonitor Monitor { get; set; }

            public void Reset( LocationRoot root, ISqlNodeLocationRange rangeFilter )
            {
                _builder.Reset( root );
                Debug.Assert( _builder.Depth == -1 );
                _filteredRange = null;
                if( (_rangeFilter = rangeFilter) != null )
                {
                    var e = rangeFilter.MergeContiguous().GetEnumerator();
                    if( e.MoveNext() ) _filteredRange = e;
                }
                _rangeFilterStatus = VisitedNodeRangeFilterStatus.None;
            }

            public void EnsureRootForNode( ISqlNode root )
            {
                if( root != _builder.Root?.Node ) _builder.Reset( new LocationRoot( root ) );
            }

            public ISqlNodeLocationRange RangeFilter => _rangeFilter;

            public VisitedNodeRangeFilterStatus RangeFilterStatus
            {
                get { return _rangeFilterStatus; }
                set { _rangeFilterStatus = value; }
            }

            public VisitedNodeRangeFilterStatus Enter( ISqlNode prev, ISqlNode n )
            {
                _rangeFilterStatus = VisitedNodeRangeFilterStatus.None;
                Tag = null;
                VisitedNode = n;
                _builder.Enter( n );
                int p = _builder.Position;

                if( _rangeFilter == null )
                {
                    _rangeFilterStatus = p == 0 
                                            ? VisitedNodeRangeFilterStatus.FIntersecting 
                                            : VisitedNodeRangeFilterStatus.FIntersecting|VisitedNodeRangeFilterStatus.FBegAfter;
                    if( p < _builder.Root.Node.Width - 1 ) _rangeFilterStatus |= VisitedNodeRangeFilterStatus.FEndBefore;
                }
                else
                {
                    int endPos;
                    if( _filteredRange == null || (endPos = p + n.Width) <= _filteredRange.Current.Beg.Position )
                    {
                        Leave( prev, true );
                    }
                    else
                    {
                        _rangeFilterStatus |= VisitedNodeRangeFilterStatus.FIntersecting;
                        int deltaBeg = _builder.Position - _filteredRange.Current.Beg.Position;
                        if( deltaBeg < 0 ) _rangeFilterStatus |= VisitedNodeRangeFilterStatus.FBegBefore;
                        else if( deltaBeg > 0 ) _rangeFilterStatus |= VisitedNodeRangeFilterStatus.FBegAfter;
                        int deltaEnd = endPos - _filteredRange.Current.End.Position;
                        if( deltaEnd < 0 ) _rangeFilterStatus |= VisitedNodeRangeFilterStatus.FEndBefore;
                        else if( deltaEnd > 0 ) _rangeFilterStatus |= VisitedNodeRangeFilterStatus.FEndAfter;
                    }
                }
                return _rangeFilterStatus;
            }

            public void Leave( ISqlNode prev, bool skipped )
            {
                _builder.Leave( VisitedNode );
                VisitedNode = prev;
                if( prev != null && _filteredRange != null )
                {
                    int p = _builder.Position;
                    while( p >= _filteredRange.Current.End.Position )
                    {
                        if( !_filteredRange.MoveNext() )
                        {
                            _filteredRange = null;
                            break;
                        }
                    }
                }
            }

            public ISqlNodeLocationManager LocationManager => _builder.Root;

            public ISqlNode VisitedNode { get; private set; }

            public object Tag { get; set; }

            public LocationRoot Root => _builder.Root;

            public int Depth => _builder.Depth;

            public int Position => _overridePos >= 0 ? _overridePos : _builder.Position;

            public void OverridePosition( int pos = -1 )
            {
                _overridePos = pos;
            }

            public SqlNodeLocation GetCurrentLocation() =>  _builder.GetCurrent();

        }

        readonly VContext _context;
        bool _hasUnParsedText;
        bool _stop;


        /// <summary>
        /// Initializes a new location visitor.
        /// </summary>
        protected SqlNodeLocationVisitor()
        {
            _context = new VContext();
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
        /// Overridden to adapt this public inherited method to the internals of this implementation.
        /// This enables a location aware visitor to be used independently of <see cref="SqlTransformHost.Visit(SqlNodeLocationVisitor, ISqlNodeLocationRange)"/>.
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
            if( rangeFilter == SqlNodeLocationRange.EmptySet ) return root.Node;
            _hasUnParsedText = false;
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
            VisitedNodeRangeFilterStatus status = _context.Enter( prev, e );
            if( status != 0 )
            {
                // We use the stack here to restore the position, the status and the Tag of the visited
                // item before calling AfterVisitItem: this enables the location builder
                // to not use a stack (the LigthLocationBuilder does not use a stack).
                int savePos = _context.Position;
                bool doChildrenVisit = BeforeVisitItem() && !_stop;
                object tag = _context.Tag;
                if( doChildrenVisit ) v = base.VisitItem( e );
                // Restores the item position by overriding it.
                _context.OverridePosition( savePos );
                _context.Tag = tag;
                _context.RangeFilterStatus = status;
                v = AfterVisitItem( v );
                // Clears the override.
                _context.OverridePosition();
                _context.Leave( prev, !doChildrenVisit );
            }
            return v;
        }

        /// <summary>
        /// Gets whether unparsed text has been injected during any previous transformation.
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
