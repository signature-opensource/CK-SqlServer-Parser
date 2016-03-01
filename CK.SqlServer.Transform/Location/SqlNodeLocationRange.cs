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
    public class SqlNodeLocationRange : ISqlNodeLocationRange, ISqlNodeLocationRangeInternal
    {
        SqlNodeLocation _beg;
        SqlNodeLocation _end;

        public static readonly SqlNodeLocationRange EmptySet = new SqlNodeLocationRange();

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
            int w = end.Position - beg.Position;
            if( w < 0 ) throw new ArgumentException( "Range: beg position is after end." );
            _beg = beg;
            _end = w == 0 ? beg : end;
        }

        public bool IsLocation => _beg == _end;

        int ISqlNodeLocationRange.Count => 1;

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
            if( w == 0 ) return _end = _beg;
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
            //TODO: handle IsLocation case... or totally remove Precision if eventually useless.
            return this;
        }

        static internal ISqlNodeLocationRange Create( IEnumerable<SqlNodeLocationRange> ranges, int countOfRanges, bool cloneOnMulti = true )
        {
            if( countOfRanges == 0 ) return EmptySet;
            if( countOfRanges == 1 ) return ranges.First();
            if( cloneOnMulti ) return new LocationRangeList( ranges.ToArray() );
            IReadOnlyList<SqlNodeLocationRange> r = ranges as IReadOnlyList<SqlNodeLocationRange>;
            return new LocationRangeList( r == null ? ranges.ToArray() : r );
        }

        internal SqlNodeLocationRange InternalSetEnd( SqlNodeLocation end )
        {
            Debug.Assert( end.Position >= _beg.Position );
            return new SqlNodeLocationRange( _beg, end );
        }

        internal SqlNodeLocationRange InternalSetBeg( SqlNodeLocation beg )
        {
            Debug.Assert( beg.Position <= _end.Position );
            return new SqlNodeLocationRange( beg, _end );
        }

        ISqlNodeLocationRangeInternal ISqlNodeLocationRangeInternal.InternalSetEnd( SqlNodeLocation end ) => InternalSetEnd( end );

        public override string ToString()
        {
            Debug.Assert( Beg != null || this == EmptySet );
            Debug.Assert( (Beg == null) == (End == null) );
            if( this == EmptySet ) return "∅";
            if( IsLocation ) return string.Format( "]{0}[",Beg.Position );
            return string.Format( "[{0},{1}[", Beg.Position, End.Position );   
        }

        internal enum Kind
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

        static internal ISqlNodeLocationRange Unified( SqlNodeLocationRange r1, SqlNodeLocationRange r2, Func<Kind,SqlNodeLocationRange,SqlNodeLocationRange,ISqlNodeLocationRange> on )
        {
            Debug.Assert( r1 != null && r1 != EmptySet && r2 != null && r2 != EmptySet );
            if( r1.Beg.Position == r2.Beg.Position )
            {
                if( r1.End.Position == r2.End.Position ) return on( Kind.Equal, r1, r2 );
                if( r1.End.Position < r2.End.Position ) return on( Kind.SameStart, r1, r2 );
                return on( Kind.SameStart|Kind.Swapped, r2, r1 );
            }
            Kind swap = 0;
            if( r1.Beg.Position > r2.Beg.Position )
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
                case Kind.Independent: return EmptySet;
            }
            throw new NotImplementedException();
        }

        static ISqlNodeLocationRange DoUnion( Kind k, SqlNodeLocationRange r1, SqlNodeLocationRange r2 )
        {
            switch( k & ~Kind.Swapped )
            {
                case Kind.Equal: return r1.MostPrecise( r2 );
                case Kind.Contained: return r1;
                case Kind.SameStart: return new SqlNodeLocationRange( r1.Beg.MostPrecise( r2.Beg ), r2.End );
                case Kind.SameEnd: return new SqlNodeLocationRange( r1.Beg, r1.End.MostPrecise( r2.End ) );
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
                case Kind.Equal:
                case Kind.Contained|Kind.Swapped: return EmptySet;

                case Kind.Congruent:
                case Kind.Independent: return r1;

                case Kind.Congruent|Kind.Swapped:
                case Kind.Independent|Kind.Swapped: return r2;

                case Kind.Contained:
                    {
                        var first = new SqlNodeLocationRange( r1.Beg, r2.Beg );
                        var last = new SqlNodeLocationRange( r2.End, r1.End );
                        return new LocationRangeCombined( first, last );
                    }

                case Kind.SameStart: return EmptySet;
                case Kind.SameStart|Kind.Swapped: return new SqlNodeLocationRange( r1.End, r2.End );

                case Kind.Overlapped:
                case Kind.SameEnd: return new SqlNodeLocationRange( r1.Beg, r2.Beg );
                case Kind.SameEnd | Kind.Swapped: return EmptySet;
                case Kind.Overlapped | Kind.Swapped: return new SqlNodeLocationRange( r1.End, r2.End ); ;
            }
            throw new NotImplementedException();
        }

        public SqlNodeLocationRange Intersect( SqlNodeLocationRange other )
        {
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            return this == EmptySet || IsLocation || other == EmptySet || other.IsLocation 
                        ? EmptySet 
                        : (SqlNodeLocationRange)Unified( this, other, DoIntersect );
        }

        public ISqlNodeLocationRange Union( SqlNodeLocationRange other )
        {
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            return this == EmptySet || IsLocation 
                        ? other 
                        : (other == EmptySet || other.IsLocation 
                                ? this 
                                : Unified( this, other, DoUnion ));
        }

        public ISqlNodeLocationRange Except( SqlNodeLocationRange other )
        {
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            return this == EmptySet || IsLocation || other == EmptySet || other.IsLocation 
                        ? this 
                        : Unified( this, other, DoExcept );
        }

        public IEnumerator<SqlNodeLocationRange> GetEnumerator() => new CKEnumeratorMono<SqlNodeLocationRange>( this );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }

}
