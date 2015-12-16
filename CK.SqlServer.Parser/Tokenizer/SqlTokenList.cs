using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;
using System.Diagnostics;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Composition of similar tokens (can be empty).
    /// Used with <see cref="SqlTokenOpenPar"/> and <see cref="SqlTokenClosePar"/>.
    /// </summary>
    /// <typeparam name="T">Token type (must be a <see cref="SqlToken"/>).</typeparam>
    public sealed class SqlTokenList<T> : SqlNode where T : SqlToken
    {
        readonly IReadOnlyList<T> _tokens;

        static readonly public SqlTokenList<T> Empty = new SqlTokenList<T>( ImmutableList<T>.Empty, null, null );

        public SqlTokenList( params T[] tokens )
        {
            _tokens = tokens;
        }

        SqlTokenList( IReadOnlyList<T> tokens, ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            _tokens = tokens;
        }

        public static SqlTokenList<T> Create( T token )
        {
            if( token == null ) throw new ArgumentNullException( "token" );
            return new SqlTokenList<T>( new[] { token }, null, null );
        }

        public static SqlTokenList<T> Create( T prefix, SqlTokenList<T> tail )
        {
            if( prefix == null ) throw new ArgumentNullException( "prefix" );
            if( tail == null ) throw new ArgumentNullException( "tail" );
            return new SqlTokenList<T>( ImmutableArray.Create( prefix ).AddRange( tail._tokens ), null, null );
        }

        public static SqlTokenList<T> Create( SqlTokenList<T> head, T suffix )
        {
            if( head == null ) throw new ArgumentNullException( "head" );
            if( suffix == null ) throw new ArgumentNullException( "suffix" );
            return new SqlTokenList<T>( ImmutableArray.CreateRange( head._tokens ).Add( suffix ), null, null );
        }

        public static SqlTokenList<T> Create( SqlTokenList<T> head, SqlTokenList<T> tail )
        {
            if( head == null ) throw new ArgumentNullException( "head" );
            if( tail == null ) throw new ArgumentNullException( "tail" );
            if( head._tokens.Count == 0 ) return tail;
            if( tail._tokens.Count == 0 ) return head;
            return new SqlTokenList<T>( ImmutableArray.CreateRange( head._tokens ).AddRange( tail._tokens ), null, null );
        }

        /// <summary>
        /// Gets the <see cref="Tokens"/>: the children of this list are its tokens.
        /// </summary>
        public override IReadOnlyList<SqlNode> ChildrenNodes => _tokens;

        /// <summary>
        /// Gets the list of tokens.
        /// </summary>
        public IReadOnlyList<T> Tokens => _tokens;

        /// <summary>
        /// Gets all tokens of this list: same as <see cref="Tokens"/>.
        /// </summary>
        public override IEnumerable<SqlToken> AllTokens => _tokens;

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<SqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTokenList<T>( _tokens == content ? _tokens : content.Cast<T>().ToReadOnlyList(), leading, trailing );
        }

        public override void WriteWithoutTrivias( ISqlTextWriter w )
        {
            foreach( var t in _tokens )
            {
                t.Write( w );
            }
        }


        [DebuggerStepThrough]
        internal protected override SqlNode Accept( SqlItemVisitor visitor )
        {
            return visitor.Visit( this );
        }
    }

}
