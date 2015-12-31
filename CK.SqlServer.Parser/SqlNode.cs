using CK.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public abstract class SqlNode : ISqlNode
    {
        protected SqlNode( ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
        {
            LeadingTrivias = leading ?? ImmutableList<SqlTrivia>.Empty;
            TrailingTrivias = trailing ?? ImmutableList<SqlTrivia>.Empty;
        }

        public abstract IReadOnlyList<ISqlNode> ChildrenNodes { get; }

        public ImmutableList<SqlTrivia> LeadingTrivias { get; }

        public ImmutableList<SqlTrivia> TrailingTrivias { get; }

        public IEnumerable<ISqlNode> LeadingNodes
        {
            get
            {
                ISqlNode n = this;
                for( ;;)
                {
                    yield return n;
                    if( n.ChildrenNodes.Count == 0 ) yield break;
                    n = n.ChildrenNodes[0];
                }
            }
        }

        public IEnumerable<ISqlNode> TrailingNodes
        {
            get
            {
                ISqlNode n = this;
                for( ;;)
                {
                    yield return n;
                    if( n.ChildrenNodes.Count == 0 ) yield break;
                    n = n.ChildrenNodes[n.ChildrenNodes.Count-1];
                }
            }
        }

        public virtual IEnumerable<SqlTrivia> FullLeadingTrivias => LeadingNodes.SelectMany( n => n.LeadingTrivias );

        public virtual IEnumerable<SqlTrivia> FullTrailingTrivias => TrailingNodes.Reverse().SelectMany( n => n.TrailingTrivias );

        public virtual IEnumerable<SqlToken> AllTokens => ChildrenNodes.ToTokens();

        ISqlNode DoLift( ImmutableList<SqlTrivia>.Builder hL, ImmutableList<SqlTrivia>.Builder tL, ISqlNode n, bool root )
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

        internal ISqlNode DoLiftLeadingTrivias()
        {
            return DoLift( ImmutableList.CreateBuilder<SqlTrivia>(), null, this, true );
        }

        internal ISqlNode DoLiftTrailingTrivias()
        {
            return DoLift( null, ImmutableList.CreateBuilder<SqlTrivia>(), this, true );
        }

        internal ISqlNode DoLiftBothTrivias()
        {
            return DoLift( ImmutableList.CreateBuilder<SqlTrivia>(), ImmutableList.CreateBuilder<SqlTrivia>(), this, true );
        }

        internal ISqlNode DoSetTrivias( ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing )
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
                    ? DoClone( leading, null, trailing )
                    : this;
        }

        /// <summary>
        /// Sets new children nodes.
        /// </summary>
        /// <param name="childrenNodes">Children nodes.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        public ISqlNode SetChildrenNodes( IReadOnlyList<ISqlNode> childrenNodes )
        {
            if( childrenNodes == null ) childrenNodes = Util.EmptyArray<ISqlNode>.Empty;
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
        public ISqlNode ReplaceChildNode( int i, ISqlNode child )
        {
            var c = ChildrenNodes.ToList();
            if( child != null )
            {
                if( c[i] == child ) return this;
                c[i] = child;
            }
            else c.RemoveAt( i );
            return DoClone( LeadingTrivias, c, TrailingTrivias );
        }

        /// <summary>
        /// Inserts or replace one or more children at a given index in <see cref="ChildrenNodes"/>.
        /// </summary>
        /// <param name="iStart">The index.</param>
        /// <param name="count">The number of children to replace.</param>
        /// <param name="child">The children to insert.</param>
        /// <returns>A new immutable object or this if no change occurred.</returns>
        public ISqlNode StuffChildren( int iStart, int count, IReadOnlyList<ISqlNode> children )
        {
            if( children == null ) throw new ArgumentNullException( nameof( children ) );
            List<ISqlNode> c = ChildrenNodes.ToList();
            c.RemoveRange( iStart, count );
            c.InsertRange( iStart, children );
            return DoClone( LeadingTrivias, c, TrailingTrivias );
        }

        internal ISqlNode DoAddLeadingTrivia( SqlTrivia t )
        {
            if( t.IsEmpty ) return this;
            return DoClone( LeadingTrivias.Insert( 0, t ), null, TrailingTrivias );
        }

        internal ISqlNode DoAddTrailingTrivia( SqlTrivia t )
        {
            if( t.IsEmpty ) return this;
            return DoClone( LeadingTrivias, null, TrailingTrivias.Add( t ) );
        }

        public virtual bool IsToken( SqlTokenType t ) => false;

        public virtual ISqlNode UnPar => this;

        /// <summary>
        /// Fundamental method that rebuilds this node with new trivias and content.
        /// </summary>
        /// <param name="leading">Leading trivias.</param>
        /// <param name="children">New content.</param>
        /// <param name="trailing">Trailing trivias.</param>
        /// <returns>A new immutable object.</returns>
        protected abstract SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing );

        /// <summary>
        /// Required because of SqlExternalNode: DoClone can not be internal protected.
        /// </summary>
        internal SqlNode InternalDoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return DoClone( leading, children, trailing );
        }

        internal protected abstract ISqlNode Accept( SqlItemVisitor visitor );

        public void Write( ISqlTextWriter w )
        {
            foreach( var t in LeadingTrivias ) w.Write( t );
            WriteWithoutTrivias( w );
            foreach( var t in TrailingTrivias ) w.Write( t );
        }

        public virtual void WriteWithoutTrivias( ISqlTextWriter w )
        {
            foreach( var t in ChildrenNodes ) t.Write( w );
        }

        /// <summary>
        /// Overridden to return a compact representation on one line 
        /// without trivias (see <see cref="SqlTextWriter.CreateOneLineCompact"/>).
        /// </summary>
        /// <returns>One line, compact, representation.</returns>
        public override string ToString()
        {
            ISqlTextWriter w = SqlTextWriter.CreateOneLineCompact();
            WriteWithoutTrivias( w );
            return w.ToString();
        }

        public string ToString( bool withThisTrivia )
        {
            ISqlTextWriter w = SqlTextWriter.CreateDefault();
            if( withThisTrivia ) Write( w );
            else WriteWithoutTrivias( w );
            return w.ToString();
        }

    }
}
