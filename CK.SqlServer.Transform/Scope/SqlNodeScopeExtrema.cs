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
    /// Builds a unique range on the extrema of the inner range(s).
    /// </summary>
    public class SqlNodeScopeExtrema : SqlNodeScopeBuilder
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
            if( inner == null ) throw new ArgumentNullException( nameof( inner ) );
            _inner = inner;
            _option = option;
        }

        protected override void DoReset()
        {
            _inner.Reset();
            _first = _last = null;
        }

        protected override ISqlNodeLocationRange DoEnter( IVisitContext context )
        {
            return Handle( _inner.Enter( context ), null );
        }

        protected override ISqlNodeLocationRange DoLeave( IVisitContext context )
        {
            return Handle( _inner.Leave( context ), null );
        }

        protected override ISqlNodeLocationRange DoConclude( IVisitContextBase context )
        {
            return Handle( _inner.Conclude( context ), context.LocationManager );
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

        protected string ToString( string inner ) => _option switch
        {
            Option.None => $"(extrema of {inner})",
            Option.AfterIncluded => $"(from the start of {inner} to the end)",
            Option.After => $"(from the end of {inner} to the end)",
            Option.Before => $"(from the start to the start of {inner})",
            _ => $"(from the start to the end of {inner})"
        };

        public override string ToString() => ToString( _inner.ToString() );

    }

}
