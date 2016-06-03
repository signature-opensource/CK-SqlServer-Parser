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
        /// Whether node location should internally be built as <see cref="SqlNodeLocation.IsQualifiedLocation"/>
        /// by all <see cref="SqlNodeLocationVisitor"/> created by this transformer.
        /// Defaults to false.
        /// </summary>
        public bool BuildQualifiedNodeLocations { get; set; }

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
        /// Gets the name space of the current <see cref="Root"/>.
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
        /// <see cref="Reparse"/> is automatcally called if needed at the 
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
            foreach( ISqlTStatement t in transformer.Body )
            {
                if( RunStatement( t, scope ) )
                {
                    Monitor.Trace().Send( $"Successfully applied '{t.ToString()}'" );
                }
                else
                {
                    using( Monitor.OpenError().Send( $"Failed to apply '{t.ToString()}' to:" ) )
                    {
                        Monitor.Trace().Send( Node.ToString( true ) );
                    }
                    return false;
                }
            }
            return NeedReparse ? Reparse() : true;
        }

        bool RunStatement( ISqlTStatement t, SqlNodeScopeBuilder scope )
        {
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
                return Apply( new Transformers.AddParameter( addParam.Parameters, pBefore, pAfter ), scope );
            }
            #endregion
            var insert = t as SqlTInject;
            if( insert != null )
            {
                UnparsedInjectInfo info = new UnparsedInjectInfo( insert );
                return new Transformers.UnparsedTextTransformer( info, scope ).Apply( this );
            }
            var replace = t as SqlTReplace;
            if( replace != null )
            {
                UnparsedInjectInfo info = new UnparsedInjectInfo( replace );
                return new Transformers.UnparsedTextTransformer( info, scope ).Apply( this );
            }
            throw new NotSupportedException( $"Transform statement '{t.ToString()}' not supported." );
        }

        /// <summary>
        /// Unconditionally reparses the root <see cref="Node"/>.
        /// </summary>
        /// <returns>True on success, false on error.</returns>
        public bool Reparse()
        {
            using( _monitor.OpenTrace().Send( "Parsing transfomrmation result." ) )
            {
                string text = _root.Node.ToString( true, true );
                ISqlNode newOne;
                var result = SqlAnalyser.Parse( out newOne, ParseMode.OneOrMoreStatements, text );
                if( result.IsError )
                {
                    using( _monitor.OpenError().Send( result.ErrorMessage ) )
                    {
                        _monitor.Trace().Send( text );
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
                    visitor.BuildQualifiedNodeLocations = BuildQualifiedNodeLocations;
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
                var s = new ScopeResolver( builder, _monitor ) { BuildQualifiedNodeLocations = BuildQualifiedNodeLocations };
                s.VisitRoot( _root, rangeFilter );
                return error ? null : s.Result;
            }
        }


    }
}
