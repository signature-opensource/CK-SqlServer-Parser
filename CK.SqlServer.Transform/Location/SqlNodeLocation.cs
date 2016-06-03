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
    /// Immutable capture of a node, its parent node and absolute position (in terms of tokens).
    /// </summary>
    public class SqlNodeLocation: IComparable<SqlNodeLocation>
    {
        /// <summary>
        /// The node can be null (ie. unknown) when this location is returned by <see cref="Successor"/> or <see cref="Predecessor"/>.
        /// </summary>
        public readonly ISqlNode Node;

        /// <summary>
        /// The node parent. Never null except for the root node.
        /// </summary>
        public readonly SqlNodeLocation Parent;

        /// <summary>
        /// The associated position.
        /// </summary>
        public readonly int Position;

        /// <summary>
        /// Initializes a new location.
        /// </summary>
        /// <param name="parent">Parent of the node. Can not be null.</param>
        /// <param name="node">The node. Null for a raw location.</param>
        /// <param name="p">The associated position.</param>
        internal SqlNodeLocation( SqlNodeLocation parent, ISqlNode node, int p )
        {
            if( parent == null ) throw new ArgumentNullException( nameof( parent ) );
            if( parent.Node == null ) throw new ArgumentNullException( "Parent node is null.", nameof( parent ) );
            Parent = parent;
            Node = node;
            Position = p;
        }

        /// <summary>
        /// Root objects internal constructor.
        /// </summary>
        internal SqlNodeLocation( ISqlNode node )
        {
            Debug.Assert( node != null );
            Node = node;
        }

        /// <summary>
        /// Gets whether this location is a raw one: only the <see cref="Position"/> is known, <see cref="Node"/> is null.
        /// When the Node is known, <see cref="IsQualifiedLocation"/> is true.
        /// </summary>
        public bool IsRawLocation => Node == null;

        /// <summary>
        /// Gets whether this location's <see cref="Node"/> is not null.
        /// </summary>
        public bool IsQualifiedLocation => Node != null;

        /// <summary>
        /// Gets whether this location is a full one: the <see cref="Node"/> is the ending token.
        /// A full location is a qualified one.
        /// </summary>
        public bool IsFullLocation => Node is SqlToken;

        /// <summary>
        /// Returns the qualified location of a node at this <see cref="Position"/>.
        /// The node must exist at this position otherwise an <see cref="ArgumentException"/> is thrown. 
        /// </summary>
        /// <returns>The qualified location.</returns>
        public SqlNodeLocation ToQualifiedLocation( ISqlNode node ) => Node == node ? this : GetRoot().GetQualifiedLocation( Position, node );

        /// <summary>
        /// Returns the full location for this <see cref="Position"/> (the full path to the token terminal node).
        /// </summary>
        /// <returns>
        /// This location if <see cref="IsFullLocation"/> is true, the full location for this <see cref="Position"/> otherwise.
        /// </returns>
        public SqlNodeLocation ToFullLocation() => IsFullLocation ? this : GetRoot().GetFullLocation( Position );


        /// <summary>
        /// Gets whether this is the special beginning marker.
        /// </summary>
        public bool IsBegMarker => Node == SqlKeyword.BegOfInput;

        /// <summary>
        /// Gets whether this is the special end marker.
        /// </summary>
        public bool IsEndMarker => Node == SqlKeyword.EndOfInput;

        /// <summary>
        /// Compares this location to another one. The comparison is based on the <see cref="Position"/>, and, when the 
        /// two positions are equal, on the parent relationships: a child (more precise) is greater than a parent.
        /// </summary>
        /// <param name="other">The location to compare to.</param>
        /// <returns>Positive if this is greater than other.</returns>
        public int CompareTo( SqlNodeLocation other )
        {
            return CompareTo( other, false );
        }

        /// <summary>
        /// Compares this location to another one. The comparison is based on the <see cref="Position"/>, and, when the 
        /// two positions are equal, on the length of the path.
        /// </summary>
        /// <param name="other">The location to compare to.</param>
        /// <param name="parentIsGreater">True to consider shorter path to be better than a longer one.</param>
        /// <returns></returns>
        public int CompareTo( SqlNodeLocation other, bool parentIsGreater )
        {
            int cmp = 1;
            if( other != null )
            {
                cmp = Position - other.Position;
                if( cmp == 0 )
                {
                    if( Node != other.Node )
                    {
                        cmp = ComparePathLength( other );
                        if( parentIsGreater ) cmp = -cmp;
                    }
                }
            }
            return cmp;
        }

        /// <summary>
        /// Compares the path length (number of non null <see cref="Node"/>).
        /// </summary>
        /// <param name="other">Other location.</param>
        /// <returns>1 if this path is longer that the other one, 0 or -1 otherwise.</returns>
        public int ComparePathLength( SqlNodeLocation other )
        {
            if( other == null ) return 1;
            var pO = other.Parent;
            if( pO != null && pO.Node != null ) pO = pO.Parent;
            var pT = Parent;
            if( pT != null && pT.Node != null ) pT = pT.Parent;
            for( ;; )
            {
                if( pO == null ) return pT == null ? 0 : 1;
                if( pT == null ) return -1;
                pO = pO.Parent;
                pT = pT.Parent;
            }
        }

        /// <summary>
        /// Gets the greatest location between this and another one. If the two <see cref="Position"/> are the same,
        /// the most precise one is returned.
        /// </summary>
        /// <param name="other">Other location to challenge.</param>
        /// <returns>This or other.</returns>
        public SqlNodeLocation Max( SqlNodeLocation other )
        {
            return CompareTo( other, false ) >= 0 ? this : other;
        }

        /// <summary>
        /// Gets the lowest location between this and another one. If the two <see cref="Position"/> are the same,
        /// the most precise one is returned.
        /// </summary>
        /// <param name="other">Other location to challenge.</param>
        /// <returns>This or other.</returns>
        public SqlNodeLocation Min( SqlNodeLocation other )
        {
            return CompareTo( other, true ) <= 0 ? this : other;
        }

        /// <summary>
        /// Returns the most precise location when the two <see cref="Position"/> are the same, otherwise this.
        /// </summary>
        /// <param name="other">The other location.</param>
        /// <returns>This or the other if positions are the same and other has a longer path than this.</returns>
        public SqlNodeLocation MostPrecise( SqlNodeLocation other )
        {
            return Position != other.Position || ComparePathLength( other ) >= 0 ? this : other;
        }

        /// <summary>
        /// Returns a location on the previous token. This is null if <see cref="IsBegMarker"/> is true.
        /// </summary>
        /// <param name="fullLocation">True to obtain the full location.</param>
        /// <returns>A location on the previous token.</returns>
        public SqlNodeLocation Predecessor( bool fullLocation = false )
        {
            if( IsBegMarker ) return null;
            if( IsEndMarker ) return GetRoot().GetLastLocation( fullLocation );
            if( Position == 0 ) return GetRoot().BegMarker;
            Debug.Assert( Parent != null, "Handled by Position == 0 above." );
            if( fullLocation ) return GetRoot().GetFullLocation( Position - 1 );
            var p = Parent;
            while( p.Position == Position ) p = p.Parent;
            return new SqlNodeLocation( p, null, Position - 1 );
        }

        /// <summary>
        /// Returns a location on the next token. This is null if <see cref="IsEndMarker"/> is true.
        /// </summary>
        /// <param name="fullLocation">True to obtain the full location.</param>
        /// <returns>A location on the next token.</returns>
        public SqlNodeLocation Successor( bool fullLocation = false )
        {
            if( IsBegMarker ) return fullLocation ? GetRoot().GetFullLocation( 0 ) : GetRoot();
            if( IsEndMarker ) return null;
            if( fullLocation ) return GetRoot().GetFullLocation( Position + 1 );
            var p = Parent;
            Debug.Assert( p != null || (Position == 0 && this is LocationRoot) );
            int newPos = Position + 1;
            if( p == null ) return Node.Width == newPos ? ((LocationRoot)this).EndMarker : new SqlNodeLocation( this, null, 1 );
            while( p.Position + p.Node.Width <= newPos )
            {
                p = p.Parent;
                if( p == null ) return GetRoot().GetRawLocation( newPos );
            }
            return new SqlNodeLocation( p, null, newPos );
        }

        /// <summary>
        /// Gets the root location node. Never null.
        /// </summary>
        public SqlNodeLocation Root => GetRoot();

        /// <summary>
        /// Changes the <see cref="Node"/> of this location that must be <see cref="IsQualifiedLocation"/>.
        /// </summary>
        /// <param name="newNode">The new node.</param>
        /// <returns>The new root node.</returns>
        public ISqlNode ChangeNode( ISqlNode newNode )
        {
            if( !IsQualifiedLocation ) throw new InvalidOperationException( "Node must be a qualified location." );
            var toChange = this;
            var oldLeaf = Node;
            var newLeaf = newNode;
            while( (toChange = toChange.Parent) != null )
            {
                newLeaf = toChange.Node.ReplaceContentNode( ( n, i ) => n == oldLeaf ? newLeaf : n );
                oldLeaf = toChange.Node;
            }
            return newLeaf;
        }


        LocationRoot GetRoot()
        {
            SqlNodeLocation l = this;
            while( l.Parent != null ) l = l.Parent;
            return (LocationRoot)l;
        }

        /// <summary>
        /// Get the path this one up to the root parent.
        /// </summary>
        public IEnumerable<SqlNodeLocation> ReversePath
        {
            get
            {
                var p = this;
                do
                {
                    yield return p;
                    p = p.Parent;
                }
                while( p != null );
            }
        }

        /// <summary>
        /// Get the path from the root parent up to this one.
        /// </summary>
        public IEnumerable<SqlNodeLocation> Path => ReversePath.Reverse();

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            int curP = -1;
            b.Append( Position ).Append( " - " );
            foreach( var l in Path )
            {
                if( curP >= 0 ) b.Append( '/' );
                b.Append( l.Node == null ? "(null node)" : l.Node.GetType().Name );
                if( curP != l.Position ) b.Append( $"[{l.Position}]" );
                curP = l.Position;
            }
            if( Node != null ) b.Append( " - " ).Append( Node.ToStringHyperCompact() );
            return b.ToString();
        }
    }

}
