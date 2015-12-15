using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CK.Core;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Abstract base class for <see cref="SqlExpr"/> (enclosable in parenthesis and base of all objects that are handled by <see cref="SqlAnalyser.ParseExpression"/>) 
    /// and <see cref="SqlNoExpr"/> (not enclosable and base class for <see cref="SqlExprBaseSt">statements</see>).
    /// It should not be specialized directly: inherit from SqlExpr or SqlNoExpr.
    /// </summary>
    public abstract class SqlItem : SqlNode
    {
        protected readonly SqlNode[] Slots;

        protected SqlItem( ImmutableList<SqlTrivia> leading, SqlNode[] slots, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            Debug.Assert( slots != null );
            Slots = slots;
        }

        public sealed override IReadOnlyList<SqlNode> ChildrenNodes => Slots;

        /// <summary>
        /// Gets the tokens that compose this item.
        /// </summary>
        public override IEnumerable<SqlToken> AllTokens => Slots.ToTokens();

        internal protected abstract T Accept<T>( ISqlItemVisitor<T> visitor );

        static internal T[] EnsureArray<T>( IEnumerable<T> content )
        {
            T[] r = content as T[];
            if( r == null )
            {
                IReadOnlyCollection<T> c = content as IReadOnlyCollection<T>;
                if( c == null ) r = content.ToArray();
                else
                {
                    int i = 0;
                    r = new T[c.Count];
                    foreach( var e in content ) r[i++] = e;
                }
            }
            return r;
        }

        static internal T[] CreateArray<T>( params T[] e )
        {
            Debug.Assert( e != null && e.All( i => i != null ) );
            return e;
        }

        static internal T[] CreateArray<T>( IEnumerable<T> content, int contentLength, T suffix )
        {
            Debug.Assert( content != null && suffix != null && contentLength <= content.Count() && contentLength >= 0 );
            var c = new T[contentLength + 1];
            int i = 0;
            foreach( var e in content )
            {
                c[i++] = e;
                if( i == contentLength ) break;
            }
            c[contentLength] = suffix;
            return c;
        }

        static internal T[] CreateArray<T>( T prefix, IEnumerable<T> content, int skippedContent, int contentLength, T suffix )
        {
            Debug.Assert( content != null && suffix != null && prefix != null 
                            && skippedContent >= 0 && contentLength >= 0 && skippedContent + contentLength <= content.Count() );
            var c = new T[++contentLength + 1];
            c[0] = prefix;
            int i = 0;
            foreach( var e in content.Skip( skippedContent ) )
            {
                if( i == contentLength ) break;
                c[++i] = e;
            }
            c[contentLength] = suffix;
            return c;
        }

        static internal SqlNode[] CreateArray( SqlTokenOpenPar openPar, IEnumerable<SqlNode> content, int contentLength, SqlTokenClosePar closePar )
        {
            Debug.Assert( contentLength == 0 || !(content.First() is SqlTokenList<SqlTokenOpenPar>) );
            return CreateArray( SqlTokenList<SqlTokenOpenPar>.Create( openPar ), content, 0, contentLength, SqlTokenList<SqlTokenClosePar>.Create( closePar ) );
        }

        static internal SqlNode[] CreateEnclosedArray( IReadOnlyList<SqlNode> content )
        {
            Debug.Assert( content.Count == 0 || !(content.First() is SqlTokenList<SqlTokenOpenPar>) );
            return CreateArray( SqlToken.EmptyOpenPar, content, 0, content.Count, SqlToken.EmptyClosePar );
        }

        static internal SqlNode[] CreateEnclosedArray( SqlTokenOpenPar prefix, IReadOnlyList<SqlNode> alreadyEnclosedComponents, SqlTokenClosePar suffix )
        {
            Debug.Assert( prefix != null && alreadyEnclosedComponents != null && suffix != null );
            Debug.Assert( alreadyEnclosedComponents.Count >= 2 );
            Debug.Assert( alreadyEnclosedComponents[0] is SqlTokenList<SqlTokenOpenPar> );
            Debug.Assert( alreadyEnclosedComponents[alreadyEnclosedComponents.Count - 1] is SqlTokenList<SqlTokenClosePar> );

            SqlTokenList<SqlTokenOpenPar> existOpen = (SqlTokenList<SqlTokenOpenPar>)alreadyEnclosedComponents[0];
            SqlTokenList<SqlTokenClosePar> existClose = (SqlTokenList<SqlTokenClosePar>)alreadyEnclosedComponents[alreadyEnclosedComponents.Count - 1];

            return CreateArray( SqlTokenList<SqlTokenOpenPar>.Create( prefix, existOpen ), alreadyEnclosedComponents, 1, alreadyEnclosedComponents.Count - 2, SqlTokenList<SqlTokenClosePar>.Create( existClose, suffix ) );
        }

    }

}
