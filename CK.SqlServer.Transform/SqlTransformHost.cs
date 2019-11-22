using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Hosts transformation of immutable <see cref="ISqlNode"/> by tracking
    /// the changed root and handling range selection.
    /// </summary>
    public class SqlTransformHost
    {
        readonly IActivityMonitor _monitor;
        LocationRoot _root;

        /// <summary>
        /// Initializes a new <see cref="SqlTransformHost"/>
        /// </summary>
        /// <param name="node">The initial root node. Can not be null.</param>
        /// <param name="monitor">The monitor to use. Can not be null.</param>
        public SqlTransformHost( ISqlNode node, IActivityMonitor monitor )
        {
            if( node == null ) throw new ArgumentNullException( nameof( node ) );
            if( monitor == null ) throw new ArgumentNullException( nameof( monitor ) );
            _root = new LocationRoot( node, false );
            _monitor = monitor;
        }

        /// <summary>
        /// External entry point (late bound).
        /// </summary>
        /// <param name="monitor">The monitor that will receive logs and errors. Can not be null.</param>
        /// <param name="transformer">The transformer to apply.</param>
        /// <param name="target">The target node to transform. Can not be null.</param>
        /// <returns>The transformed node on success and null if an error occurred.</returns>
        static public ISqlNode Transform( IActivityMonitor monitor, SqlTransformer transformer, ISqlNode target )
        {
            if( transformer == null ) throw new ArgumentNullException( nameof( transformer ) );
            var h = new SqlTransformHost( target, monitor );
            if( !h.Apply( transformer ) ) return null;
            var r = h.Node;
            // Attempts to keep the external target type if possible:
            // The Script case is the only one we can handle: it is a statement list
            // and is parsed back to unique statement.
            // Any other type change can not be handled.
            if( target is SqlStatementList && r is ISqlStatement )
            {
                r = new SqlStatementList( new[] { (ISqlStatement)r } );
            }
            return r;
        }

        /// <summary>
        /// Gets the current node. 
        /// This property tracks the transformed node.
        /// </summary>
        public ISqlNode Node
        {
            get { return _root.Node; }
            set
            {
                if( value == null ) throw new ArgumentNullException();
                if( value != _root.Node )
                {
                    _root = new LocationRoot( value, false );
                }
            }
        }

        /// <summary>
        /// Gets the name space of the current root <see cref="Node"/>.
        /// </summary>
        public ISqlNodeLocationManager CurrentNamespace => _root;
        
        /// <summary>
        /// Gets the monitor used by this transformer.
        /// </summary>
        public IActivityMonitor Monitor => _monitor;

        /// <summary>
        /// Gets or sets whether the root <see cref="Node"/> should be reparsed.
        /// This is automatically set to true by some visitors that plays with unparsed texts.
        /// </summary>
        public bool NeedReparse { get; set; }

        /// <summary>
        /// Applies a <see cref="SqlTransformer"/> to <see cref="Node"/>.
        /// <see cref="Reparse"/> is automatically called if needed at the 
        /// end of the transformation.
        /// </summary>
        /// <param name="transformer">The transformer. Can not be null.</param>
        /// <param name="scope">An optional scope for the transformation.</param>
        /// <returns>True on success, false on error.</returns>
        public bool Apply( SqlTransformer transformer, SqlNodeScopeBuilder scope = null )
        {
            if( transformer == null ) throw new ArgumentNullException( nameof( transformer ) );
            if( transformer.TargetFullName != null )
            {
                var target = new SqlNodeScopeBreadthPredicate( n => n is ISqlFullNameHolder && ((ISqlFullNameHolder)n).FullName.ToStringHyperCompact() == transformer.TargetFullName.ToStringHyperCompact() );
                if( scope == null ) scope = target;
                else
                {
                    scope = new SqlNodeScopeIntersect( scope, target );
                }
            }
            if( !RunStatements( transformer.Body, scope ) ) return false;
            return NeedReparse ? Reparse() : true;
        }

        bool RunStatements( SqlTStatementList list, SqlNodeScopeBuilder scope )
        {
            foreach( ISqlTStatement t in list )
            {
                if( RunStatement( t, scope ) )
                {
                    if( !(t is SqlTInScope) ) Monitor.Trace( $"Successfully applied '{t.ToString()}'." );
                }
                else
                {
                    Monitor.Error( $"Failed to apply '{t.ToString()}'." );
                    return false;
                }
            }
            return true;
        }

        bool RunStatement( ISqlTStatement t, SqlNodeScopeBuilder scope )
        {
            var inject = t as SqlTInject;
            if( inject != null )
            {
                UnparsedInjectInfo info = new UnparsedInjectInfo( inject );
                return new Transformers.UnparsedTextTransformer( info, scope ).Apply( this );
            }
            var injectInto = t as SqlTInjectInto;
            if( injectInto != null )
            {
                return Apply( new Transformers.TriviaExtensionPointInjectVisitor( Monitor, injectInto ), scope );
            }
            var replace = t as SqlTReplace;
            if( replace != null )
            {
                UnparsedInjectInfo info = new UnparsedInjectInfo( replace );
                return new Transformers.UnparsedTextTransformer( info, scope ).Apply( this );
            }
            var inScope = t as SqlTInScope;
            #region SqlTScope
            if( inScope != null )
            {
                Debug.Assert( inScope.Location is ISqlTLocationFinder || inScope.Location is SqlTRangeLocationFinder );
                Debug.Assert( inScope.Body is SqlTStatementList || inScope.Body is SqlTInScope );
                for( ; ; )
                {
                    SqlNodeScopeBuilder newScope;
                    if( inScope.Location is ISqlTLocationFinder oneOrMultiFinder )
                    {
                        var loc = oneOrMultiFinder.GetFinderInfo();
                        Debug.Assert( (loc.IsNodeMatchRange && loc.PatternRange != null) != ((loc.IsNodeMatchPart || loc.IsNodeMatchStatement) && loc.NodeMatcher != null) );
                        newScope = loc.CreateScopeBuilder();
                        // Intersects with the potential current scope.
                        if( scope != null ) newScope = new SqlNodeScopeIntersect( scope, newScope );
                        // Applies the cardinality specification.
                        newScope = new SqlNodeScopeCardinalityFilter( newScope, loc.Card );
                    }
                    else
                    {
                        SqlTRangeLocationFinder r = (SqlTRangeLocationFinder)inScope.Location;

                        var locFirstMatch = r.FirstLocation.GetFinderInfo();
                        SqlNodeScopeBuilder matchFirst = locFirstMatch.TriviaMatcher != null
                                                            ? new SqlNodeScopeFromTriviaMatcher( r.AfterOrBeforeOrBetweenT.TokenType != SqlTokenType.Before,
                                                                                                 locFirstMatch.TriviaMatcher,
                                                                                                 locFirstMatch.ToString() )
                                                            : locFirstMatch.CreateScopeBuilder();

                        if( r.AfterOrBeforeOrBetweenT.TokenType == SqlTokenType.After )
                        {
                            newScope = matchFirst as SqlNodeScopeExtrema ?? new SqlNodeScopeExtrema( matchFirst, SqlNodeScopeExtrema.Option.After );
                        }
                        else if( r.AfterOrBeforeOrBetweenT.TokenType == SqlTokenType.Before )
                        {
                            newScope = matchFirst as SqlNodeScopeExtrema ?? new SqlNodeScopeExtrema( matchFirst, SqlNodeScopeExtrema.Option.Before );
                        }
                        else
                        {
                            Debug.Assert( r.AfterOrBeforeOrBetweenT.TokenType == SqlTokenType.Between );
                            LocationInfo locSecondMatch = r.SecondLocation.GetFinderInfo();
                            SqlNodeScopeBuilder matchSecond = locFirstMatch.TriviaMatcher != null
                                                                ? new SqlNodeScopeFromTriviaMatcher( false, locSecondMatch.TriviaMatcher, locSecondMatch.ToString() )
                                                                : locSecondMatch.CreateScopeBuilder();

                            matchFirst = matchFirst as SqlNodeScopeExtrema ?? new SqlNodeScopeExtrema( matchFirst, SqlNodeScopeExtrema.Option.AfterIncluded );
                            matchSecond = matchSecond as SqlNodeScopeExtrema ?? new SqlNodeScopeExtrema( matchSecond, SqlNodeScopeExtrema.Option.BeforeIncluded );
                            newScope = new SqlNodeScopeIntersect( matchFirst, matchSecond );
                        }
                        // Intersects with the potential current scope.
                        if( scope != null ) newScope = new SqlNodeScopeIntersect( scope, newScope );
                    }
                    scope = newScope;
                    if( inScope.Body is SqlTStatementList statements )
                    {
                        return RunStatements( statements, scope );
                    }
                    inScope = (SqlTInScope)inScope.Body;
                }
            }
            #endregion
            var addParam = t as SqlTAddParameter;
            #region SqlTAddParameter
            if( addParam != null )
            {
                // When we write "add parameter @P int before @E" then @E must appear AFTER the
                // inserted parameters. 
                string pBefore = null, pAfter = null;
                if( addParam.AfterOrBeforeT != null )
                {
                    if( addParam.AfterOrBeforeT.IsToken( SqlTokenType.After ) )
                    {
                        pBefore = addParam.ParameterName.Name;
                    }
                    else
                    {
                        pAfter = addParam.ParameterName.Name;
                    }
                }
                if( NeedReparse && !Reparse() ) return false;
                return Apply( new Transformers.AddParameter( addParam.Parameters, pBefore, pAfter ), scope );
            }
            #endregion
            var addColumn = t as SqlTAddColumn;
            #region SqlTAddColumn
            if( addColumn != null )
            {
                if( NeedReparse && !Reparse() ) return false;
                return Apply( new Transformers.AddColumn( addColumn.Columns ), scope );
            }
            #endregion
            var combineSelect = t as SqlTCombineSelect;
            #region SqlTCombineSelect
            if( combineSelect != null )
            {
                throw new NotImplementedException( "combinig selects has yet to be done." );
            }
            #endregion

            throw new NotSupportedException( $"Transform statement '{t.ToString()}' not supported." );
        }

        /// <summary>
        /// Unconditionally reparses the root <see cref="Node"/>.
        /// </summary>
        /// <returns>True on success, false on error.</returns>
        public bool Reparse()
        {
            using( _monitor.OpenTrace( "Parsing transformation result." ) )
            {
                string text = _root.Node.ToString( true, true );
                ISqlNode newOne;
                var result = SqlAnalyser.Parse( out newOne, ParseMode.OneOrMoreStatements, text );
                if( result.IsError )
                {
                    using( _monitor.OpenError( result.ErrorMessage ) )
                    {
                        _monitor.Trace( text );
                    }
                    return false;
                }
                Node = newOne;
                NeedReparse = false;
                return true;
            }
        }

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
            return Visit( transformer, filter );
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
        /// <returns>True on success, false on error.</returns>
        public bool Visit( SqlNodeLocationVisitor visitor, ISqlNodeLocationRange rangeFilter = null )
        {
            if( visitor == null ) throw new ArgumentNullException( nameof( visitor ) );
            bool success = true;
            using( _monitor.OnError( () => success = false ) )
            {
                if( visitor.Monitor == null ) visitor.Monitor = _monitor;
                using( visitor.Monitor != _monitor
                        ? visitor.Monitor.Output.CreateBridgeTo( _monitor.Output.BridgeTarget )
                        : null )
                {
                    ISqlNode r = visitor.VisitRoot( _root, rangeFilter );
                    if( r != _root.Node && success )
                    {
                        _root = new LocationRoot( r, false );
                        NeedReparse |= visitor.HasUnParsedText;
                    }
                }
            }
            return success;
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
