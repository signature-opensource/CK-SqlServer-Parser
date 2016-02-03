using CK.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public abstract partial class SqlNode : ISqlNode
    {
        protected SqlNode( ImmutableList<SqlTrivia> leading = null, ImmutableList<SqlTrivia> trailing = null )
        {
            LeadingTrivias = leading ?? ImmutableList<SqlTrivia>.Empty;
            TrailingTrivias = trailing ?? ImmutableList<SqlTrivia>.Empty;
        }

        public abstract IReadOnlyList<ISqlNode> ChildrenNodes { get; }

        public abstract IList<ISqlNode> GetRawContent();

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
            IList<ISqlNode> content = n.GetRawContent();
            bool contentChanged = false;
            int nbC = content.Count;
            if( nbC > 0 )
            {
                int idx;
                if( nbC == 1 || hL != null )
                {
                    ISqlNode firstChild = RawGetFirstChildInContent( content, out idx );
                    if( firstChild != null )
                    {
                        contentChanged = RawReplaceContentNode( content, idx, DoLift( hL, nbC == 1 ? tL : null, firstChild, false ) ) != null;
                    }
                }
                if( nbC > 1 && tL != null )
                {
                    ISqlNode lastChild = RawGetLastChildInContent( content, out idx );
                    if( lastChild != null )
                    {
                        contentChanged |= RawReplaceContentNode( content, idx, DoLift( null, tL, lastChild, false ) ) != null;
                    }
                }
            }
            if( !contentChanged ) content = null;
            if( tL != null ) tL.AddRange( n.TrailingTrivias );
            SqlNode sN = (SqlNode)n;
            return root 
                    ? sN.InternalDoClone( 
                            hL != null ? hL.ToImmutableList() : n.LeadingTrivias, 
                            content, 
                            tL != null ? tL.ToImmutableList() : n.TrailingTrivias ) 
                    : sN.InternalDoClone( 
                            hL != null ? ImmutableList<SqlTrivia>.Empty : n.LeadingTrivias, 
                            content, 
                            tL != null ? ImmutableList<SqlTrivia>.Empty : n.TrailingTrivias );
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

        internal ISqlNode DoExtractTrailingTrivias( Func<SqlTrivia, bool> predicate )
        {
            int nb = TrailingTrivias.Count;
            int keep;
            if( (keep = nb) != 0 )
            {
                foreach( var t in TrailingTrivias.Reverse() )
                {
                    if( !predicate( t ) ) break;
                    --keep;
                }
            }
            if( keep == 0 )
            {
                IList<ISqlNode> content = GetRawContent();
                int idx;
                ISqlNode c = RawGetLastChildInContent( content, out idx );
                if( c != null )
                {
                    content = RawReplaceContentNode( content, idx, c.ExtractTrailingTrivias( predicate ) );
                }
                else
                {
                    if( nb == 0 ) return this;
                    content = null;
                }
                return DoClone( LeadingTrivias, content, ImmutableList<SqlTrivia>.Empty );
            }
            else if( keep != nb )
            {
                return DoClone( LeadingTrivias, null, TrailingTrivias.RemoveRange( keep, nb - keep ) );
            }
            return this;
        }

        internal ISqlNode DoExtractLeadingTrivias( Func<SqlTrivia, bool> filter )
        {
            int nb = LeadingTrivias.Count;
            int keep;
            if( (keep = nb) != 0 )
            {
                foreach( var t in LeadingTrivias )
                {
                    if( !filter( t ) ) break;
                    --keep;
                }
            }
            if( keep == 0 )
            {
                IList<ISqlNode> content = GetRawContent();
                int idx;
                ISqlNode c = RawGetFirstChildInContent( content, out idx );
                if( c != null )
                {
                    content = RawReplaceContentNode( content, idx, c.ExtractLeadingTrivias( filter ) );
                }
                else
                {
                    if( nb == 0 ) return this;
                    content = null;
                } 
                return DoClone( ImmutableList<SqlTrivia>.Empty, content, TrailingTrivias );
            }
            else if( keep != nb )
            {
                return DoClone( LeadingTrivias.RemoveRange( keep, nb - keep ), null, TrailingTrivias );
            }
            return this;
        }

        internal ISqlNode DoSetRawContent( IList<ISqlNode> childrenNodes )
        {
            if( childrenNodes == null ) childrenNodes = Util.EmptyArray<ISqlNode>.Empty;
            return DoClone( LeadingTrivias, childrenNodes, TrailingTrivias );
        }

        internal ISqlNode DoReplaceContentNode( int i, ISqlNode child )
        {
            var c = RawReplaceContentNode( GetRawContent(), i, child );
            return c != null ? DoClone( LeadingTrivias, c, TrailingTrivias ) : this;
        }

        internal ISqlNode DoReplaceContentNode( int i1, ISqlNode child1, int i2, ISqlNode child2 )
        {
            var c = RawReplaceContentNode( GetRawContent(), i1, child1, i2, child2 );
            return c != null ? DoClone( LeadingTrivias, c, TrailingTrivias ) : this;
        }

        internal ISqlNode DoStuffRawContent( int iStart, int count, IReadOnlyList<ISqlNode> children )
        {
            if( children == null ) throw new ArgumentNullException( nameof( children ) );
            IList<ISqlNode> c = GetRawContent();
            RawStuffContent( c, iStart, count, children );
            return DoClone( LeadingTrivias, c, TrailingTrivias );
        }

        static IList<ISqlNode> RawReplaceContentNode( IList<ISqlNode> content, int i, ISqlNode child )
        {
            if( child != null || content is ISqlNode[] )
            {
                if( content[i] == child ) return null;
                content[i] = child;
            }
            else content.RemoveAt( i );
            return content;
        }

        static IList<ISqlNode> RawReplaceContentNode( IList<ISqlNode> content, int i1, ISqlNode child1, int i2, ISqlNode child2 )
        {
            if( (child1 != null && child2 != null) || content is ISqlNode[] )
            {
                if( content[i1] == child1 && content[i2] == child2 ) return null;
                content[i1] = child1;
                content[i2] = child2;
            }
            else
            {
                if( child1 == null )
                {
                    content.RemoveAt( i1 );
                    if( i1 < i2 ) --i2;
                }
                else content[i1] = child1;

                if( child2 == null ) content.RemoveAt( i2 );
                else content[i2] = child2;
            }
            return content;
        }

        static ISqlNode RawGetFirstChildInContent( IList<ISqlNode> content, out int idx )
        {
            ISqlNode firstChild = null;
            for( idx = 0; idx < content.Count; ++idx )
                if( (firstChild = content[idx]) != null ) break;
            return firstChild;
        }

        static ISqlNode RawGetLastChildInContent( IList<ISqlNode> content, out int idx )
        {
            ISqlNode lastChild = null;
            for( idx = content.Count - 1; idx >= 0; --idx )
                if( (lastChild = content[idx]) != null ) break;
            return lastChild;
        }

        static IList<ISqlNode> RawStuffContent( IList<ISqlNode> content, int iStart, int count, IReadOnlyList<ISqlNode> children )
        {
            List<ISqlNode> lC = content as List<ISqlNode>;
            if( lC == null || children.Count == count )
            {
                Debug.Assert( lC == null || content is ISqlNode[] );
                bool changed = false;
                for( int i = 0; i < count; ++i )
                {
                    if( content[iStart + i] != children[i] )
                    {
                        content[iStart + i] = children[i];
                        changed = true;
                    }
                }
                return changed ? content : null;
            }
            if( lC == null ) throw new InvalidOperationException();
            lC.RemoveRange( iStart, count );
            lC.InsertRange( iStart, children );
            return content;
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
        /// <param name="content">New content.</param>
        /// <param name="trailing">Trailing trivias.</param>
        /// <returns>A new immutable object.</returns>
        protected abstract SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing );

        /// <summary>
        /// Required because of SqlExternalNode: DoClone can not be internal protected.
        /// </summary>
        internal SqlNode InternalDoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return leading == LeadingTrivias && content == null && trailing == TrailingTrivias
                    ? this
                    : DoClone( leading, content, trailing );
        }

        internal protected abstract ISqlNode Accept( SqlNodeVisitor visitor );

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

        public string ToString( bool withThisTrivia, bool restoreUselessComments = false )
        {
            ISqlTextWriter w = SqlTextWriter.CreateDefault( new StringBuilder(), restoreUselessComments );
            if( withThisTrivia ) Write( w );
            else WriteWithoutTrivias( w );
            return w.ToString();
        }

    }
}
