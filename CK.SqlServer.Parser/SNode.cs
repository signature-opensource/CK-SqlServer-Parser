using System;
using System.Collections;
using System.Collections.Generic;

namespace CK.SqlServer.Parser
{
    struct SNode<T1,T2,T3> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
        where T3 : ISqlNode
    {
        public readonly T1 O1;
        public readonly T2 O2;
        public readonly T3 O3;

        public SNode( T1 o1, T2 o2, T3 o3 )
        {
            O1 = o1;
            O2 = o2;
            O3 = o3;
        }
        public ISqlNode this[int index]
        {
            get
            {
                switch( index )
                {
                    case 0: return O1;
                    case 1: return O2;
                    case 2: return O3;
                }
                throw new IndexOutOfRangeException();
            }
        }

        public int Count => 3;

        public IEnumerator<ISqlNode> GetEnumerator()
        {
            yield return O1;
            yield return O2;
            yield return O3;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
