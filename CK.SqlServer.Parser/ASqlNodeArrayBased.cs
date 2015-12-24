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
    /// </summary>
    public abstract class ASqlNodeArrayBased : SqlNode
    {
        protected readonly ISqlNode[] Children;

        protected ASqlNodeArrayBased( ImmutableList<SqlTrivia> leading, ISqlNode[] children, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            Debug.Assert( children != null );
            Children = children;
        }

        public sealed override IReadOnlyList<ISqlNode> ChildrenNodes => Children;

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

        static internal ISqlNode[] CreateArray( SqlTokenOpenPar openPar, IEnumerable<ISqlNode> content, int contentLength, SqlTokenClosePar closePar )
        {
            Debug.Assert( contentLength == 0 || !(content.First() is SqlTokenList<SqlTokenOpenPar>) );
            return CreateArray( SqlTokenList<SqlTokenOpenPar>.Create( openPar ), content, 0, contentLength, SqlTokenList<SqlTokenClosePar>.Create( closePar ) );
        }

        static internal ISqlNode[] CreateEnclosedArray( IReadOnlyList<ISqlNode> content )
        {
            Debug.Assert( content.Count == 0 || !(content.First() is SqlTokenList<SqlTokenOpenPar>) );
            return CreateArray( SqlToken.EmptyOpenPar, content, 0, content.Count, SqlToken.EmptyClosePar );
        }

        static internal ISqlNode[] CreateEnclosedArray( SqlTokenOpenPar prefix, IReadOnlyList<ISqlNode> alreadyEnclosedComponents, SqlTokenClosePar suffix )
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
