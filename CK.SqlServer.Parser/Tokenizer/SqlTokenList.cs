using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Composition of similar tokens (can be empty).
    /// Used with <see cref="SqlTokenOpenPar"/> and <see cref="SqlTokenClosePar"/>.
    /// </summary>
    /// <typeparam name="T">Token type (must be a <see cref="SqlToken"/>).</typeparam>
    public sealed class SqlTokenList<T> : SqlNode, ISqlItem where T : SqlToken
    {
        readonly ImmutableList<T> _tokens;

        static readonly public SqlTokenList<T> Empty = new SqlTokenList<T>( ImmutableList<T>.Empty, null, null );

        public SqlTokenList( params T[] tokens )
        {
            _tokens = ImmutableList.CreateRange( tokens );
        }

        SqlTokenList( ImmutableList<T> tokens, ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            _tokens = tokens;
        }

        public static SqlTokenList<T> Create( T token )
        {
            if( token == null ) throw new ArgumentNullException( "token" );
            return new SqlTokenList<T>( ImmutableList.Create( token ), null, null );
        }

        public static SqlTokenList<T> Create( T prefix, SqlTokenList<T> tail )
        {
            if( prefix == null ) throw new ArgumentNullException( "prefix" );
            if( tail == null ) throw new ArgumentNullException( "tail" );
            return new SqlTokenList<T>( tail._tokens.Insert( 0, prefix ), null, null );
        }

        public static SqlTokenList<T> Create( SqlTokenList<T> head, T suffix )
        {
            if( head == null ) throw new ArgumentNullException( "head" );
            if( suffix == null ) throw new ArgumentNullException( "suffix" );
            return new SqlTokenList<T>( head._tokens.Add( suffix ), null, null );
        }

        public static SqlTokenList<T> Create( SqlTokenList<T> head, SqlTokenList<T> tail )
        {
            if( head == null ) throw new ArgumentNullException( "head" );
            if( tail == null ) throw new ArgumentNullException( "tail" );
            if( head._tokens.IsEmpty ) return tail;
            if( tail._tokens.IsEmpty ) return head;
            return new SqlTokenList<T>( tail._tokens.AddRange( tail._tokens ), null, null );
        }

        public ImmutableList<T> Tokens { get { return _tokens; } }

        IEnumerable<SqlToken> ISqlItem.Tokens  { get { return _tokens; } }

        public override SqlNode SetTrivias( ImmutableList<SqlTrivia> leading, ImmutableList<SqlTrivia> trailing )
        {
            return TriviasDiffer( ref leading, ref trailing )
                    ? new SqlTokenList<T>( _tokens, leading, trailing )
                    : this;
        }

        public SqlToken LastOrEmptyT { get { return _tokens.IsEmpty ? SqlToken.Empty : _tokens[_tokens.Count-1]; } }

        public SqlToken FirstOrEmptyT { get { return _tokens.IsEmpty ? SqlToken.Empty : _tokens[0]; ; } }

        //public T this[int index]
        //{
        //    get { return _tokens[index]; }
        //}

        //public int Count
        //{
        //    get { return _tokens.Count; }
        //}

        //public IEnumerator<T> GetEnumerator()
        //{
        //    return _tokens.GetEnumerator();
        //}

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    return _tokens.GetEnumerator();
        //}

        public override string ToString()
        {
            var b = new StringBuilder();
            _tokens.WriteTokensWithoutTrivias( String.Empty, b );
            return b.ToString();
        }
    }

}
