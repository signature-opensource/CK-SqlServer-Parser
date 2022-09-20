using CK.Core;
using System;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Builds a unique range on the extrema of the inner range(s).
    /// </summary>
    public sealed class SqlNodeScopeExtrema : SqlNodeScopeBuilder
    {
        /// <summary>
        /// Parameter for the extrema detection.
        /// </summary>
        public enum Option
        {
            /// <summary>
            /// The extrema are the smallest and greatest locations of the inner ranges.
            /// </summary>
            None,

            /// <summary>
            /// The final range is from the very first node of the root up to the
            /// smallest start of the inner ranges.  
            /// </summary>
            Before,

            /// <summary>
            /// The final range is from the very first node of the root up to the greatest end of the inner ranges.  
            /// </summary>
            BeforeIncluded,

            /// <summary>
            /// The final range is from the smallest start of the inner ranges up to the last node of the root.
            /// </summary>
            AfterIncluded,

            /// <summary>
            /// The final range is from the greatest end of the inner ranges up to the last node of the root.  
            /// </summary>
            After
        }

        readonly SqlNodeScopeBuilder _inner;
        readonly Option _option;
        SqlNodeLocation _first;
        SqlNodeLocation _last;

        public SqlNodeScopeExtrema( SqlNodeScopeBuilder inner, Option option )
        {
            Throw.CheckNotNullArgument( inner );
            _inner = inner.GetSafeBuilder();
            _option = option;
        }

        private protected override SqlNodeScopeBuilder Clone() => new SqlNodeScopeExtrema( _inner, _option );

        private protected override void DoReset()
        {
            _inner.Reset();
            _first = _last = null;
        }

        private protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            var r = _inner.Enter( context );
            var f = Handle( r, null );
            ActivityMonitor.StaticLogger.Debug( $"Extrema {_option} Enter: {r} => {f}" );
            return f;
        }

        private protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            var r = _inner.Leave( context );
            var f = Handle( r, null );
            ActivityMonitor.StaticLogger.Debug( $"Extrema {_option} Leave: {r} => {f}" );
            return f;
        }

        private protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            var r = _inner.Conclude( context );
            var f = Handle( r, context.LocationManager );
            ActivityMonitor.StaticLogger.Debug( $"Extrema {_option} Conclude: {r} => {f}" );
            return f;
        }

        ISqlNodeLocationRange Handle( ISqlNodeLocationRange r, ISqlNodeLocationManager locationManager )
        {
            if( r != null )
            {
                _first = r.First.Beg.Min( _first );
                _last = r.Last.End.Max( _last );
            }
            if( locationManager != null )
            {
                if( _first == null ) return null;
                switch( _option )
                {
                    case Option.AfterIncluded:
                        _last = locationManager.EndMarker;
                        break;
                    case Option.After:
                        _first = _last;
                        _last = locationManager.EndMarker;
                        break;
                    case Option.Before:
                        _last = _first;
                        _first = locationManager.GetFullLocation( 0 );
                        break;
                    case Option.BeforeIncluded:
                        _first = locationManager.GetFullLocation( 0 );
                        break;
                }
                return _first.IsEndMarker || _first.Position == _last.Position
                        ? null
                        : new SqlNodeLocationRange( _first, _last );
            }
            return null;
        }

        string ToString( string inner ) => _option switch
        {
            Option.None => $"(extrema of {inner})",
            Option.AfterIncluded => $"(from the start of {inner} to the end)",
            Option.After => $"(from the end of {inner} to the end)",
            Option.Before => $"(from the start to the start of {inner})",
            _ => $"(from the start to the end of {inner})"
        };

        /// <summary>
        /// Overridden to return the description of this builder.
        /// </summary>
        /// <returns>The description.</returns>
        public override string ToString() => ToString( _inner.ToString() );

    }

}
