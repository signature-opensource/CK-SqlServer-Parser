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

        static internal T[] EnsureArray<T>( IEnumerable<T> content )
        {
            T[] r = content as T[];
            if( r == null && content != null )
            {
                IList<T> l = content as IList<T>;
                if( l != null )
                {
                    r = new T[l.Count];
                    l.CopyTo( r, 0 );
                }
                else
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
            }
            return r;
        }

    }

}
