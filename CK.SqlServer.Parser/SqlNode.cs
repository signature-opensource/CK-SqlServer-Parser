using CK.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Base class for all Sql nodes.
    /// This is an immutable object that carries leading and trailing <see cref="SqlTrivia"/>.
    /// </summary>
    public abstract class SqlNode
    {
        protected SqlNode( ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
        {
            LeadingTrivias = leading ?? ImmutableList<SqlTrivia>.Empty;
            TrailingTrivias = trailing ?? ImmutableList<SqlTrivia>.Empty;
        }

        /// <summary>
        /// Gets the direct children if any. Never null.
        /// </summary>
        public abstract IReadOnlyList<SqlNode> ChildrenNodes { get; }

        /// <summary>
        /// Leading <see cref="SqlTrivia"/>. Never null but can be empty.
        /// </summary>
        public readonly ImmutableList<SqlTrivia> LeadingTrivias;

        /// <summary>
        /// Trailing <see cref="SqlTrivia"/>. Never null but can be empty.
        /// </summary>
        public readonly ImmutableList<SqlTrivia> TrailingTrivias;

        /// <summary>
        /// Gets the leading nodes from this one to the deepest left-most children.
        /// </summary>
        public IEnumerable<SqlNode> LeadingNodes
        {
            get
            {
                var n = this;
                for( ;;)
                {
                    yield return n;
                    if( n.ChildrenNodes.Count == 0 ) yield break;
                    n = n.ChildrenNodes[0];
                }
            }
        }

        /// <summary>
        /// Gets the trailing nodes from this one to the deepest right-most children.
        /// </summary>
        public IEnumerable<SqlNode> TrailingNodes
        {
            get
            {
                var n = this;
                for( ;;)
                {
                    yield return n;
                    if( n.ChildrenNodes.Count == 0 ) yield break;
                    n = n.ChildrenNodes[n.ChildrenNodes.Count-1];
                }
            }
        }

        /// <summary>
        /// Gets the whole leading trivias for this node and its <see cref="LeadingNodes"/>.
        /// </summary>
        public IEnumerable<SqlTrivia> FullLeadingTrivias => LeadingNodes.SelectMany( n => n.LeadingTrivias );

        /// <summary>
        /// Gets the whole trailing trivias for this node and its <see cref="TrailingNodes"/>.
        /// </summary>
        public IEnumerable<SqlTrivia> FullTrailingTrivias => TrailingNodes.Reverse().SelectMany( n => n.TrailingTrivias );

        SqlNode DoLift( ImmutableList<SqlTrivia>.Builder hL, ImmutableList<SqlTrivia>.Builder tL, SqlNode n, bool root )
        {
            if( hL != null ) hL.AddRange( n.LeadingTrivias );
            int nbC = n.ChildrenNodes.Count;
            if( nbC > 0 )
            {
                if( nbC == 1 || hL != null )
                {
                    n = n.ReplaceChildNode( 0, DoLift( hL, nbC == 1 ? tL : null, n.ChildrenNodes[0], false ) );
                }
                if( nbC > 1 && tL != null )
                {
                    n = n.ReplaceChildNode( nbC - 1, DoLift( null, tL, n.ChildrenNodes[nbC - 1], false ) );
                }
            }
            if( tL != null ) tL.AddRange( n.TrailingTrivias );
            return root 
                    ? n.SetTrivias( hL != null ? hL.ToImmutableList() : n.LeadingTrivias, tL != null ? tL.ToImmutableList() : n.TrailingTrivias ) 
                    : n.SetTrivias( hL != null ? null : n.LeadingTrivias, tL != null ? null : n.TrailingTrivias );
        }

        /// <summary>
        /// Lifts leading trivias: <see cref="LeadingNodes"/> do not have leading trivias any more.
        /// </summary>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        public SqlNode LiftLeadingTrivias()
        {
            return DoLift( ImmutableList.CreateBuilder<SqlTrivia>(), null, this, true );
        }

        /// <summary>
        /// Lifts trailing trivias: <see cref="TrailingNodes"/> do not have trailing trivias any more.
        /// </summary>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        public SqlNode LiftTrailingTrivias()
        {
            return DoLift( null, ImmutableList.CreateBuilder<SqlTrivia>(), this, true );
        }

        /// <summary>
        /// Lifts leading and trailing trivias: <see cref="TrailingNodes"/> and <see cref="LeadingNodes"/> do not 
        /// have trailing trivias any more.
        /// </summary>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        public SqlNode LiftBothTrivias()
        {
            return DoLift( ImmutableList.CreateBuilder<SqlTrivia>(), ImmutableList.CreateBuilder<SqlTrivia>(), this, true );
        }

        /// <summary>
        /// Sets trivias around this node.
        /// </summary>
        /// <param name="leading">Leading trivia. Can be null for empty trivias.</param>
        /// <param name="trailing">Trailing trivia. Can be null for empty trivias.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        public SqlNode SetTrivias( ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing )
        {
            if( leading == null ) leading = ImmutableList<SqlTrivia>.Empty;
            if( trailing == null ) trailing = ImmutableList<SqlTrivia>.Empty;
            if( leading != LeadingTrivias
                && leading.Count == LeadingTrivias.Count
                && leading.SequenceEqual( LeadingTrivias ) )
            {
                leading = LeadingTrivias;
            }
            if( trailing != TrailingTrivias 
                && trailing.Count == TrailingTrivias.Count 
                && trailing.SequenceEqual( TrailingTrivias ) )
            {
                trailing = TrailingTrivias;
            }
            return leading != LeadingTrivias || trailing != TrailingTrivias
                    ? DoClone( leading, ChildrenNodes, trailing )
                    : this;
        }

        /// <summary>
        /// Sets new children nodes.
        /// </summary>
        /// <param name="childrenNodes">Children nodes.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        public SqlNode SetChildrenNodes( IReadOnlyList<SqlNode> childrenNodes )
        {
            if( childrenNodes == null ) childrenNodes = Util.EmptyArray<SqlNode>.Empty;
            return childrenNodes.Count == ChildrenNodes.Count && childrenNodes.SequenceEqual( ChildrenNodes )
                    ? this
                    : DoClone( LeadingTrivias, childrenNodes, TrailingTrivias );
        }

        /// <summary>
        /// Sets or removes a child at a given index in <see cref="ChildrenNodes"/>.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <param name="child">Null to remove or the node to replace.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        public SqlNode ReplaceChildNode( int i, SqlNode child )
        {
            var c = ChildrenNodes.ToList();
            if( child != null )
            {
                if( c[i] == child ) return this;
                c[i] = child;
            }
            else c.RemoveAt( i );
            return DoClone( LeadingTrivias, c.ToArray(), TrailingTrivias );
        }

        /// <summary>
        /// Inserts a child at a given index in <see cref="ChildrenNodes"/>.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <param name="child">Null to remove or the node to replace.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        public SqlNode InsertChildNode( int i, SqlNode child )
        {
            if( child == null ) throw new ArgumentNullException( nameof( child ) );
            var c = ChildrenNodes.ToList();
            c.Insert( i, child );
            return DoClone( LeadingTrivias, c.ToArray(), TrailingTrivias );
        }

        /// <summary>
        /// Adds a leading trivia.
        /// </summary>
        /// <param name="t">The trivia to add in front.</param>
        /// <returns>A new immutable object.</returns>
        public SqlNode AddLeadingTrivia( SqlTrivia t )
        {
            return DoClone( LeadingTrivias.Insert( 0, t ), ChildrenNodes, TrailingTrivias );
        }

        /// <summary>
        /// Adds a trailing trivia.
        /// </summary>
        /// <param name="t">The trivia to append.</param>
        /// <returns>A new immutable object.</returns>
        public SqlNode AddTrailingTrivia( SqlTrivia t )
        {
            return DoClone( LeadingTrivias, ChildrenNodes, TrailingTrivias.Add( t ) );
        }

        /// <summary>
        /// Fundamental method that rebuilds this node with new trivias and content.
        /// </summary>
        /// <param name="leading">Leading trivias.</param>
        /// <param name="children">New content.</param>
        /// <param name="trailing">Trailing trivias.</param>
        /// <returns>A new immutable object.</returns>
        protected abstract SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing );

        internal SqlNode InternalClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return DoClone( leading, children, trailing );
        }

        /// <summary>
        /// Writes the node with its <see cref="LeadingTrivia"/> and <see cref="TrailingTrivia"/>.
        /// </summary>
        /// <param name="b">The <see cref="StringBuilder"/> to write to.</param>
        /// <param name="option">The option for the trivias.</param>
        public void Write( StringBuilder b, SqlTriviaWriteOption option = SqlTriviaWriteOption.None )
        {
            LeadingTrivias.Write( b, option );
            DoWrite( b, option );
            TrailingTrivias.Write( b, option );
        }

        /// <summary>
        /// Writes the token without this leading nor traling trivias.
        /// </summary>
        /// <param name="b">The <see cref="StringBuilder"/> to write to.</param>
        /// <param name="option">The option for the trivias.</param>
        public void WriteWithoutTrivias( StringBuilder b, SqlTriviaWriteOption option = SqlTriviaWriteOption.None )
        {
            DoWrite( b, option );
        }

        /// <summary>
        /// When implemented by concrete specialization, this must write the node itself 
        /// without this <see cref="LeadingTrivias"/> and <see cref="TrailingTrivias"/>.
        /// </summary>
        /// <param name="b">The <see cref="StringBuilder"/> to write to.</param>
        /// <param name="option">The option for the trivias.</param>
        abstract protected void DoWrite( StringBuilder b, SqlTriviaWriteOption option );

        /// <summary>
        /// Overriden to return the result of <see cref="WriteWithoutTrivias"/> with an 
        /// option set to <see cref="SqlTriviaWriteOption.OneSpace"/>.
        /// </summary>
        /// <returns>The mere node.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            DoWrite( b, SqlTriviaWriteOption.OneSpace );
            return b.ToString();
        }

        /// <summary>
        /// Returns the result of <see cref="Write"/> or <see cref="WriteWithoutTrivias"/> with an 
        /// option set to <see cref="SqlTriviaWriteOption.None"/>: all internal trivias appear.
        /// </summary>
        /// <returns>This node text representation.</returns>
        public string ToString( bool withThisTrivia )
        {
            StringBuilder b = new StringBuilder();
            if( withThisTrivia ) Write( b );
            else DoWrite( b, SqlTriviaWriteOption.None );
            return b.ToString();
        }
    }
}
