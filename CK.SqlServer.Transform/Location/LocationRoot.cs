using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform
{
    class LocationRoot : SqlNodeLocation, ISqlNodeLocationManager
    {
        SqlNodeLocation[] _fullCache;
        Dictionary<ISqlNode, SqlNodeLocation> _qualifiedCache;

        public SqlNodeLocation BegMarker { get; }

        public SqlNodeLocation EndMarker { get; }

        public LocationRoot( ISqlNode node, bool cacheFullLocations = true, bool cacheQualifiedLocations = true )
            : base( node )
        {
            if( node == null ) throw new ArgumentNullException( nameof( node ) );
            BegMarker = new SqlNodeLocation( this, SqlKeyword.BegOfInput, -1 );
            EndMarker = new SqlNodeLocation( this, SqlKeyword.EndOfInput, node.Width );
            if( cacheFullLocations ) _fullCache = new SqlNodeLocation[node.Width];
            if( cacheQualifiedLocations ) _qualifiedCache = new Dictionary<ISqlNode, SqlNodeLocation>();
        }

        public SqlNodeLocation GetLastLocation( bool fullLocation ) => fullLocation ? GetFullLocation( Node.Width - 1 ) : GetRawLocation( Node.Width - 1 );

        public SqlNodeLocation GetRawLocation( int position )
        {
            if( position < 0 ) return BegMarker;
            if( position >= Node.Width ) return EndMarker;
            if( position == 0 ) return this;
            return new SqlNodeLocation( this, null, position );
        }

        public SqlNodeLocation GetFullLocation( int position )
        {
            if( position < 0 ) return BegMarker;
            if( position >= Node.Width ) return EndMarker;
            if( _fullCache != null )
            {
                return _fullCache[position] ?? (_fullCache[position] = ComputeFullLocation( position ));
            }
            return ComputeFullLocation( position );
        }

        public SqlNodeLocation GetQualifiedLocation( int position, ISqlNode node )
        {
            SqlNodeLocation loc;
            if( node == Node ) return this;
            if( _qualifiedCache != null && _qualifiedCache.TryGetValue( node, out loc ) ) return loc;
            loc = GetFullLocation( position );
            if( loc != null )
            {
                if( loc.Node == node ) return loc;
                loc = loc.Parent;
                if( node.Width == 0 )
                {
                    loc = new SqlNodeLocation( loc, node, position );
                    if( _qualifiedCache != null ) _qualifiedCache.Add( node, loc );
                    return loc;
                }
                do
                {
                    if( loc.Node == node ) return loc;
                    loc = loc.Parent;
                    Debug.Assert( loc == null || _qualifiedCache == null || _qualifiedCache.ContainsKey( loc.Node ), "Already cached if qualified cache exists." );
                }
                while( loc != null );
            }
            throw new ArgumentException( "Node does not exist at this position." );
        }

        SqlNodeLocation ComputeFullLocation( int position )
        {
            Debug.Assert( position >= 0 && position < Node.Width );
            SqlNodeLocation loc = this;
            var token = Node.LocateToken( position, ( n, p ) =>
            {
                SqlNodeLocation newLoc;
                if( _qualifiedCache != null )
                {
                    if( !_qualifiedCache.TryGetValue( n, out newLoc ) )
                    {
                        _qualifiedCache.Add( n, (newLoc = new SqlNodeLocation( loc, n, p )) );
                    }
                }
                else newLoc = new SqlNodeLocation( loc, n, p );
                loc = newLoc;
            } );
            Debug.Assert( token != Node || loc == this );
            return loc == this && token == Node ? loc : new SqlNodeLocation( loc, token, position );
        }

        internal SqlNodeLocation EnsureLocation( SqlNodeLocation parent, ISqlNode node, int pos )
        {
            if( node is SqlToken )
            {
                if( _fullCache != null )
                {
                    return _fullCache[pos] ?? (_fullCache[pos] = new SqlNodeLocation( parent, node, pos ));
                }
            }
            else
            {
                if( _qualifiedCache != null )
                {
                    SqlNodeLocation loc;
                    if( !_qualifiedCache.TryGetValue( node, out loc ) )
                    {
                        _qualifiedCache.Add( node, (loc = new SqlNodeLocation( parent, node, pos )) );
                    }
                    return loc;
                }
            }
            return new SqlNodeLocation( parent, node, pos );
        }
    }

}
