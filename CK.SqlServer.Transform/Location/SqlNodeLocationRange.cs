using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    /// <summary>
    /// Semantically immutable object. <see cref="Beg"/> and <see cref="End"/> are actually mutable in 
    /// terms of path (but not in terms of positions): the goal is, whenever possible, to capture better, 
    /// more precise, postions.
    /// </summary>
    public class SqlNodeLocationRange : ISqlNodeLocationRange
    {
        SqlNodeLocation _beg;
        SqlNodeLocation _end;

        public static readonly ISqlNodeLocationRange Empty = new SqlNodeLocationRange();

        public SqlNodeLocation Beg => _beg;

        public SqlNodeLocation End => _end;

        private SqlNodeLocationRange()
        {
            _beg = _end = null;
        }

        public SqlNodeLocationRange( SqlNodeLocation beg, SqlNodeLocation end )
        {
            if( beg == null ) throw new ArgumentNullException( nameof( beg ) );
            if( beg.IsBegMarker ) throw new ArgumentException( "Range can not include the BegMarker.", nameof( beg ) );
            if( end == null ) throw new ArgumentNullException( nameof( end ) );
            if( beg.Position >= end.Position ) throw new ArgumentException( "Range: beg position is on or after end." );
            _beg = beg;
            _end = end;
        }

        SqlNodeLocationRange ISqlNodeLocationRange.First => this;

        SqlNodeLocationRange ISqlNodeLocationRange.Last => this;

        /// <summary>
        /// Gets the most precise location that covers this range.
        /// </summary>
        /// <returns>The most precise node' location that covers this range.</returns>
        public SqlNodeLocation GetCoveringLocation()
        {
            var b = Beg.ToFullLocation();
            if( b != _beg ) _beg = b;
            int w = End.Position - b.Position;
            // Here b can never be null since Beg can not be the BegMarker: the width
            // is at most the root node's width.
            while( b.Node.Width < w ) b = b.Parent;
            return b;
        }

        /// <summary>
        /// Returns <see cref="GetCoveringLocation"/> only if it exactly covers this range.
        /// </summary>
        /// <returns>The exact qualified location or null.</returns>
        public SqlNodeLocation GetExactCoveringLocation()
        {
            SqlNodeLocation c = GetCoveringLocation();
            return c.Node.Width == End.Position -_beg.Position ? c : null;
        }

        /// <summary>
        /// Returns the most precise range when the position of <see cref="Beg"/> and <see cref="End"/> are 
        /// the same, otherwise this.
        /// </summary>
        /// <param name="other">The other range.</param>
        /// <returns>This, other or a more precise range at the same position.</returns>
        public SqlNodeLocationRange MostPrecise( SqlNodeLocationRange other )
        {
            if( other == null || other == Empty ) return this;
            var eqBeg = Beg.MostPrecise( other.Beg );
            if( eqBeg != Beg ) _beg = eqBeg;
            else other._beg = eqBeg;
            var eqEnd = End.MostPrecise( other.End );
            if( eqEnd != End ) _end = eqEnd;
            else other._end = eqEnd;
            if( eqBeg == Beg && eqEnd == End ) return this;
            if( eqBeg == other.Beg && eqEnd == other.End ) return other;
            return new SqlNodeLocationRange( eqBeg, eqEnd );
        }

        static internal ISqlNodeLocationRange Create( IReadOnlyList<SqlNodeLocationRange> ranges, bool cloneOnMulti = true )
        {
            if( ranges.Count == 0 ) return Empty;
            if( ranges.Count == 1 ) return ranges.First();
            return new LocationRangeList( cloneOnMulti ? ranges.ToArray() : ranges );
        }

        public override string ToString()
        {
            Debug.Assert( Beg != null || this == Empty );
            Debug.Assert( (Beg == null) == (End == null) );
            if( this == Empty ) return "∅";
            return string.Format( "[{0},{1}[", Beg.Position, End.Position );   
        }

        internal void InternalExtend( SqlNodeLocation end )
        {
            Debug.Assert( end.Position > _end.Position );
            _end = end;
        }

        enum Kind
        {
            Equal,
            SameEnd,
            SameStart,
            Congruent,
            Independent,
            Overlapped,
            Contained,
            Swapped = 32
        }

        static ISqlNodeLocationRange Unified( SqlNodeLocationRange r1, SqlNodeLocationRange r2, Func<Kind,SqlNodeLocationRange,SqlNodeLocationRange,ISqlNodeLocationRange> on )
        {
            if( r2 == null ) throw new ArgumentNullException( "other" );
            if( r1.Beg.Position == r2.Beg.Position )
            {
                if( r1.End.Position == r2.End.Position ) return on( Kind.Equal, r1, r2 );
                if( r1.End.Position < r2.End.Position ) return on( Kind.SameStart, r1, r2 );
                return on( Kind.SameStart|Kind.Swapped, r2, r1 );
            }
            Kind swap = 0;
            if( r2.Beg.Position > r1.Beg.Position )
            {
                var rTemp = r2;
                r2 = r1;
                r1 = rTemp;
                swap = Kind.Swapped;
            }
            if( r1.End.Position == r2.End.Position )
            {
                return on( Kind.SameEnd | swap, r1, r2 );
            }
            if( r1.End.Position == r2.Beg.Position )
            {
                return on( Kind.Congruent | swap, r1, r2 );
            }
            if( r1.End.Position < r2.Beg.Position )
            {
                return on( Kind.Independent | swap, r1, r2 );
            }
            if( r1.End.Position > r2.End.Position )
            {
                return on( Kind.Contained | swap, r1, r2 );
            }
            return on( Kind.Overlapped | swap, r1, r2 );
        }

        static ISqlNodeLocationRange DoIntersect( Kind k, SqlNodeLocationRange r1, SqlNodeLocationRange r2 )
        {
            switch( k & ~Kind.Swapped )
            {
                case Kind.Equal: return r1.MostPrecise( r2 );
                case Kind.Contained: return r2;
                case Kind.SameStart: return r1.Beg.ComparePathLength( r2.Beg ) >= 0 ? r1 : new SqlNodeLocationRange( r2.Beg, r1.End );
                case Kind.SameEnd: return r2.End.ComparePathLength( r1.End ) >= 0 ? r2 : new SqlNodeLocationRange( r2.Beg, r1.End );
                case Kind.Overlapped: return new SqlNodeLocationRange( r2.Beg, r1.End );
                case Kind.Congruent:
                case Kind.Independent: return Empty;
            }
            throw new NotImplementedException();
        }

        static ISqlNodeLocationRange DoUnion( Kind k, SqlNodeLocationRange r1, SqlNodeLocationRange r2 )
        {
            switch( k & ~Kind.Swapped )
            {
                case Kind.Equal: return r1.MostPrecise( r2 );
                case Kind.Contained: return r1;
                case Kind.SameStart: return new SqlNodeLocationRange( r1.Beg.MostPrecise( r2.Beg ), r1.End.Max( r2.End ) );
                case Kind.SameEnd: return new SqlNodeLocationRange( r1.Beg.Min( r2.Beg ), r1.End.MostPrecise( r2.End ) );
                case Kind.Overlapped:
                case Kind.Congruent: return new SqlNodeLocationRange( r1.Beg, r2.End );
                case Kind.Independent: return new LocationRangeCombined( r1, r2 );
            }
            throw new NotImplementedException();
        }

        static ISqlNodeLocationRange DoExcept( Kind k, SqlNodeLocationRange r1, SqlNodeLocationRange r2 )
        {
            switch( k )
            {
                case Kind.Equal: return Empty;
                case Kind.Congruent:
                case Kind.Congruent|Kind.Swapped:
                case Kind.Independent: return r1;
                case Kind.Independent | Kind.Swapped: return r2;
                case Kind.Contained:
                    {
                        var left = new SqlNodeLocationRange( r1.Beg, r2.Beg.Successor() );
                        var right = new SqlNodeLocationRange( r2.End.Predecessor(), r1.End );
                        return new LocationRangeCombined( left, right );
                    }
                case Kind.Contained|Kind.Swapped: return Empty;

                case Kind.SameStart: return Empty;
                case Kind.SameStart|Kind.Swapped: return new SqlNodeLocationRange( r1.End.Predecessor(), r2.End );

                case Kind.Overlapped:
                case Kind.SameEnd: return new SqlNodeLocationRange( r1.Beg, r2.Beg.Successor() );
                case Kind.SameEnd | Kind.Swapped: return Empty;
                case Kind.Overlapped | Kind.Swapped: return new SqlNodeLocationRange( r2.End.Predecessor(), r1.End ); ;
            }
            throw new NotImplementedException();
        }

        public SqlNodeLocationRange Intersect( SqlNodeLocationRange other )
        {
            return (SqlNodeLocationRange)Unified( this, other, DoIntersect );
        }

        public ISqlNodeLocationRange Union( SqlNodeLocationRange other )
        {
            return Unified( this, other, DoUnion );
        }

        public ISqlNodeLocationRange Except( SqlNodeLocationRange other )
        {
            return Unified( this, other, DoExcept );
        }

        public IEnumerator<SqlNodeLocationRange> GetEnumerator() => new CKEnumeratorMono<SqlNodeLocationRange>( this );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }

}
