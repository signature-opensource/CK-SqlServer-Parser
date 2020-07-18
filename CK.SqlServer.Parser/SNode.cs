using CK.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CK.SqlServer.Parser
{

    static class SNode
    {
        public static IEnumerator<ISqlNode> CreateFilteredEnumerator( params ISqlNode[] v )
        {
            return v.Where( e => e != null ).GetEnumerator();
        }

    }

    readonly struct SNode<T> : IReadOnlyList<ISqlNode>
        where T : class, ISqlNode
    {
        public readonly T V;

        public SNode( T o )
        {
            V = o;
        }

        public SNode( IEnumerable<ISqlNode> items )
        {
            V = (T)items.FirstOrDefault();
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index != 0 || V == null ) throw new IndexOutOfRangeException();
                return V;
            }
        }

        public int Count => V != null ? 1 : 0;

        public IEnumerator<ISqlNode> GetEnumerator()
        {
            return V != null ? new CKEnumeratorMono<ISqlNode>( V ) : Enumerable.Empty<ISqlNode>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V };
    }

    sealed class CKEnumeratorBi<T> : IEnumerator<T>
    {
        int _pos;
        readonly T _v1;
        readonly T _v2;

        public CKEnumeratorBi( T v1, T v2 )
        {
            _v1 = v1;
            _v2 = v2;
            _pos = -1;
        }

        public void Dispose() { }

        public bool MoveNext() => ++_pos <= 1;

        public void Reset() => _pos = -1;

        public T Current => _pos == 0 ? _v1 : (_pos == 1 ? _v2 : Invalid());

        object IEnumerator.Current => Current;

        static T Invalid() { throw new InvalidOperationException(); }
    }

    readonly struct SNode<T1, T2> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;

        public SNode( T1 o1, T2 o2 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( !x.MoveNext() ) return;
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}.",
                typeof( T1 ).Name, typeof( T2 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? V2;
                    default: return V2;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator()
        {
            if( Count == 0 ) return Enumerable.Empty<ISqlNode>().GetEnumerator();
            if( Count == 1 ) return new CKEnumeratorMono<ISqlNode>( V1 != null ? (ISqlNode)V1 : V2 );
            return new CKEnumeratorBi<ISqlNode>( V1, V2 );
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2 };

    }

    readonly struct SNode<T1, T2, T3> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
        where T3 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;

        public SNode( T1 o1, T2 o2, T3 o3 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( !x.MoveNext() ) return;
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}.",
                typeof( T1 ).Name, typeof( T2 ).Name, typeof( T3 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? V3;
                    case 1: return (ISqlNode)V2 ?? V3;
                    default: return V3;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3 };

    }

    readonly struct SNode<T1, T2, T3, T4> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
        where T3 : ISqlNode
        where T4 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;
        public readonly T4 V4;

        public SNode( T1 o1, T2 o2, T3 o3, T4 o4 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
            if( (V4 = o4) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( x.MoveNext() )
                            {
                                if( (V4 = (T4)x.Current) != null ) ++Count;
                                if( !x.MoveNext() ) return;
                            }
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}, {3}.",
                typeof( T1 ).Name, typeof( T2 ).Name, typeof( T3 ).Name, typeof( T4 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? (ISqlNode)V3 ?? V4;
                    case 1: return (ISqlNode)V2 ?? (ISqlNode)V3 ?? V4;
                    case 2: return (ISqlNode)V3 ?? V4;
                    default: return V4;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3, V4 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3, V4 };
    }

    readonly struct SNode<T1, T2, T3, T4, T5> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
        where T3 : ISqlNode
        where T4 : ISqlNode
        where T5 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;
        public readonly T4 V4;
        public readonly T5 V5;

        public SNode( T1 o1, T2 o2, T3 o3, T4 o4, T5 o5 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
            if( (V4 = o4) != null ) ++Count;
            if( (V5 = o5) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( x.MoveNext() )
                            {
                                if( (V4 = (T4)x.Current) != null ) ++Count;
                                if( x.MoveNext() )
                                {
                                    if( (V5 = (T5)x.Current) != null ) ++Count;
                                    if( !x.MoveNext() ) return;
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}, {3}, {4}.",
                typeof( T1 ).Name, typeof( T2 ).Name, typeof( T3 ).Name, typeof( T4 ).Name, typeof( T5 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? V5;
                    case 1: return (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? V5;
                    case 2: return (ISqlNode)V3 ?? (ISqlNode)V4 ?? V5;
                    case 3: return (ISqlNode)V4 ?? V5;
                    default: return V5;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3, V4, V5 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3, V4, V5 };
    }

    readonly struct SNode<T1, T2, T3, T4, T5, T6> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
        where T3 : ISqlNode
        where T4 : ISqlNode
        where T5 : ISqlNode
        where T6 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;
        public readonly T4 V4;
        public readonly T5 V5;
        public readonly T6 V6;

        public SNode( T1 o1, T2 o2, T3 o3, T4 o4, T5 o5, T6 o6 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
            if( (V4 = o4) != null ) ++Count;
            if( (V5 = o5) != null ) ++Count;
            if( (V6 = o6) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( x.MoveNext() )
                            {
                                if( (V4 = (T4)x.Current) != null ) ++Count;
                                if( x.MoveNext() )
                                {
                                    if( (V5 = (T5)x.Current) != null ) ++Count;
                                    if( x.MoveNext() )
                                    {
                                        if( (V6 = (T6)x.Current) != null ) ++Count;
                                        if( !x.MoveNext() ) return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}, {3}, {4}, {5}.",
                typeof( T1 ).Name, typeof( T2 ).Name, typeof( T3 ).Name, typeof( T4 ).Name, typeof( T5 ).Name, typeof( T6 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? V6;
                    case 1: return (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? V6;
                    case 2: return (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? V6;
                    case 3: return (ISqlNode)V4 ?? (ISqlNode)V5 ?? V6;
                    case 4: return (ISqlNode)V5 ?? V6;
                    default: return V6;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3, V4, V5, V6 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3, V4, V5, V6 };
    }

    readonly struct SNode<T1, T2, T3, T4, T5, T6, T7> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
        where T3 : ISqlNode
        where T4 : ISqlNode
        where T5 : ISqlNode
        where T6 : ISqlNode
        where T7 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;
        public readonly T4 V4;
        public readonly T5 V5;
        public readonly T6 V6;
        public readonly T7 V7;

        public SNode( T1 o1, T2 o2, T3 o3, T4 o4, T5 o5, T6 o6, T7 o7 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
            if( (V4 = o4) != null ) ++Count;
            if( (V5 = o5) != null ) ++Count;
            if( (V6 = o6) != null ) ++Count;
            if( (V7 = o7) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( x.MoveNext() )
                            {
                                if( (V4 = (T4)x.Current) != null ) ++Count;
                                if( x.MoveNext() )
                                {
                                    if( (V5 = (T5)x.Current) != null ) ++Count;
                                    if( x.MoveNext() )
                                    {
                                        if( (V6 = (T6)x.Current) != null ) ++Count;
                                        if( x.MoveNext() )
                                        {
                                            if( (V7 = (T7)x.Current) != null ) ++Count;
                                            if( !x.MoveNext() ) return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}, {3}, {4}, {5}, {6}.",
                typeof( T1 ).Name, typeof( T2 ).Name, typeof( T3 ).Name, typeof( T4 ).Name, typeof( T5 ).Name, typeof( T6 ).Name, typeof( T7 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? V7;
                    case 1: return (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? V7;
                    case 2: return (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? V7;
                    case 3: return (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? V7;
                    case 4: return (ISqlNode)V5 ?? (ISqlNode)V6 ?? V7;
                    case 5: return (ISqlNode)V6 ?? V7;
                    default: return V7;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3, V4, V5, V6, V7 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3, V4, V5, V6, V7 };
    }

    readonly struct SNode<T1, T2, T3, T4, T5, T6, T7, T8> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
        where T3 : ISqlNode
        where T4 : ISqlNode
        where T5 : ISqlNode
        where T6 : ISqlNode
        where T7 : ISqlNode
        where T8 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;
        public readonly T4 V4;
        public readonly T5 V5;
        public readonly T6 V6;
        public readonly T7 V7;
        public readonly T8 V8;

        public SNode( T1 o1, T2 o2, T3 o3, T4 o4, T5 o5, T6 o6, T7 o7, T8 o8 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
            if( (V4 = o4) != null ) ++Count;
            if( (V5 = o5) != null ) ++Count;
            if( (V6 = o6) != null ) ++Count;
            if( (V7 = o7) != null ) ++Count;
            if( (V8 = o8) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( x.MoveNext() )
                            {
                                if( (V4 = (T4)x.Current) != null ) ++Count;
                                if( x.MoveNext() )
                                {
                                    if( (V5 = (T5)x.Current) != null ) ++Count;
                                    if( x.MoveNext() )
                                    {
                                        if( (V6 = (T6)x.Current) != null ) ++Count;
                                        if( x.MoveNext() )
                                        {
                                            if( (V7 = (T7)x.Current) != null ) ++Count;
                                            if( x.MoveNext() )
                                            {
                                                if( (V8 = (T8)x.Current) != null ) ++Count;
                                                if( !x.MoveNext() ) return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}.",
                typeof( T1 ).Name,
                typeof( T2 ).Name,
                typeof( T3 ).Name,
                typeof( T4 ).Name,
                typeof( T5 ).Name,
                typeof( T6 ).Name,
                typeof( T7 ).Name,
                typeof( T8 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? V8;
                    case 1: return (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? V8;
                    case 2: return (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? V8;
                    case 3: return (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? V8;
                    case 4: return (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? V8;
                    case 5: return (ISqlNode)V6 ?? (ISqlNode)V7 ?? V8;
                    case 6: return (ISqlNode)V7 ?? V8;
                    default: return V8;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3, V4, V5, V6, V7, V8 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3, V4, V5, V6, V7, V8 };
    }

    readonly struct SNode<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
        where T3 : ISqlNode
        where T4 : ISqlNode
        where T5 : ISqlNode
        where T6 : ISqlNode
        where T7 : ISqlNode
        where T8 : ISqlNode
        where T9 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;
        public readonly T4 V4;
        public readonly T5 V5;
        public readonly T6 V6;
        public readonly T7 V7;
        public readonly T8 V8;
        public readonly T9 V9;

        public SNode( T1 o1, T2 o2, T3 o3, T4 o4, T5 o5, T6 o6, T7 o7, T8 o8, T9 o9 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
            if( (V4 = o4) != null ) ++Count;
            if( (V5 = o5) != null ) ++Count;
            if( (V6 = o6) != null ) ++Count;
            if( (V7 = o7) != null ) ++Count;
            if( (V8 = o8) != null ) ++Count;
            if( (V9 = o9) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( x.MoveNext() )
                            {
                                if( (V4 = (T4)x.Current) != null ) ++Count;
                                if( x.MoveNext() )
                                {
                                    if( (V5 = (T5)x.Current) != null ) ++Count;
                                    if( x.MoveNext() )
                                    {
                                        if( (V6 = (T6)x.Current) != null ) ++Count;
                                        if( x.MoveNext() )
                                        {
                                            if( (V7 = (T7)x.Current) != null ) ++Count;
                                            if( x.MoveNext() )
                                            {
                                                if( (V8 = (T8)x.Current) != null ) ++Count;
                                                if( x.MoveNext() )
                                                {
                                                    if( (V9 = (T9)x.Current) != null ) ++Count;
                                                    if( !x.MoveNext() ) return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}.",
                typeof( T1 ).Name,
                typeof( T2 ).Name,
                typeof( T3 ).Name,
                typeof( T4 ).Name,
                typeof( T5 ).Name,
                typeof( T6 ).Name,
                typeof( T7 ).Name,
                typeof( T8 ).Name,
                typeof( T9 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? V9;
                    case 1: return (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? V9;
                    case 2: return (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? V9;
                    case 3: return (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? V9;
                    case 4: return (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? V9;
                    case 5: return (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? V9;
                    case 6: return (ISqlNode)V7 ?? (ISqlNode)V8 ?? V9;
                    case 7: return (ISqlNode)V8 ?? V9;
                    default: return V9;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3, V4, V5, V6, V7, V8, V9 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3, V4, V5, V6, V7, V8, V9 };
    }

    readonly struct SNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
        where T3 : ISqlNode
        where T4 : ISqlNode
        where T5 : ISqlNode
        where T6 : ISqlNode
        where T7 : ISqlNode
        where T8 : ISqlNode
        where T9 : ISqlNode
        where T10 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;
        public readonly T4 V4;
        public readonly T5 V5;
        public readonly T6 V6;
        public readonly T7 V7;
        public readonly T8 V8;
        public readonly T9 V9;
        public readonly T10 V10;

        public SNode( T1 o1, T2 o2, T3 o3, T4 o4, T5 o5, T6 o6, T7 o7, T8 o8, T9 o9, T10 o10 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
            if( (V4 = o4) != null ) ++Count;
            if( (V5 = o5) != null ) ++Count;
            if( (V6 = o6) != null ) ++Count;
            if( (V7 = o7) != null ) ++Count;
            if( (V8 = o8) != null ) ++Count;
            if( (V9 = o9) != null ) ++Count;
            if( (V10 = o10) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( x.MoveNext() )
                            {
                                if( (V4 = (T4)x.Current) != null ) ++Count;
                                if( x.MoveNext() )
                                {
                                    if( (V5 = (T5)x.Current) != null ) ++Count;
                                    if( x.MoveNext() )
                                    {
                                        if( (V6 = (T6)x.Current) != null ) ++Count;
                                        if( x.MoveNext() )
                                        {
                                            if( (V7 = (T7)x.Current) != null ) ++Count;
                                            if( x.MoveNext() )
                                            {
                                                if( (V8 = (T8)x.Current) != null ) ++Count;
                                                if( x.MoveNext() )
                                                {
                                                    if( (V9 = (T9)x.Current) != null ) ++Count;
                                                    if( x.MoveNext() )
                                                    {
                                                        if( (V10 = (T10)x.Current) != null ) ++Count;
                                                        if( !x.MoveNext() ) return;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}.",
                typeof( T1 ).Name,
                typeof( T2 ).Name,
                typeof( T3 ).Name,
                typeof( T4 ).Name,
                typeof( T5 ).Name,
                typeof( T6 ).Name,
                typeof( T7 ).Name,
                typeof( T8 ).Name,
                typeof( T9 ).Name,
                typeof( T10 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? V10;
                    case 1: return (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? V10;
                    case 2: return (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? V10;
                    case 3: return (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? V10;
                    case 4: return (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? V10;
                    case 5: return (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? V10;
                    case 6: return (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? V10;
                    case 7: return (ISqlNode)V8 ?? (ISqlNode)V9 ?? V10;
                    case 8: return (ISqlNode)V9 ?? V10;
                    default: return V10;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3, V4, V5, V6, V7, V8, V9, V10 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3, V4, V5, V6, V7, V8, V9, V10 };
    }

    readonly struct SNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IReadOnlyList<ISqlNode>
        where T1 : ISqlNode
        where T2 : ISqlNode
        where T3 : ISqlNode
        where T4 : ISqlNode
        where T5 : ISqlNode
        where T6 : ISqlNode
        where T7 : ISqlNode
        where T8 : ISqlNode
        where T9 : ISqlNode
        where T10 : ISqlNode
        where T11 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;
        public readonly T4 V4;
        public readonly T5 V5;
        public readonly T6 V6;
        public readonly T7 V7;
        public readonly T8 V8;
        public readonly T9 V9;
        public readonly T10 V10;
        public readonly T11 V11;

        public SNode( T1 o1, T2 o2, T3 o3, T4 o4, T5 o5, T6 o6, T7 o7, T8 o8, T9 o9, T10 o10, T11 o11 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
            if( (V4 = o4) != null ) ++Count;
            if( (V5 = o5) != null ) ++Count;
            if( (V6 = o6) != null ) ++Count;
            if( (V7 = o7) != null ) ++Count;
            if( (V8 = o8) != null ) ++Count;
            if( (V9 = o9) != null ) ++Count;
            if( (V10 = o10) != null ) ++Count;
            if( (V11 = o11) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( x.MoveNext() )
                            {
                                if( (V4 = (T4)x.Current) != null ) ++Count;
                                if( x.MoveNext() )
                                {
                                    if( (V5 = (T5)x.Current) != null ) ++Count;
                                    if( x.MoveNext() )
                                    {
                                        if( (V6 = (T6)x.Current) != null ) ++Count;
                                        if( x.MoveNext() )
                                        {
                                            if( (V7 = (T7)x.Current) != null ) ++Count;
                                            if( x.MoveNext() )
                                            {
                                                if( (V8 = (T8)x.Current) != null ) ++Count;
                                                if( x.MoveNext() )
                                                {
                                                    if( (V9 = (T9)x.Current) != null ) ++Count;
                                                    if( x.MoveNext() )
                                                    {
                                                        if( (V10 = (T10)x.Current) != null ) ++Count;
                                                        if( x.MoveNext() )
                                                        {
                                                            if( (V11 = (T11)x.Current) != null ) ++Count;
                                                            if( !x.MoveNext() ) return;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}.",
                typeof( T1 ).Name,
                typeof( T2 ).Name,
                typeof( T3 ).Name,
                typeof( T4 ).Name,
                typeof( T5 ).Name,
                typeof( T6 ).Name,
                typeof( T7 ).Name,
                typeof( T8 ).Name,
                typeof( T9 ).Name,
                typeof( T10 ).Name,
                typeof( T11 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? V11;
                    case 1: return (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? V11;
                    case 2: return (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? V11;
                    case 3: return (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? V11;
                    case 4: return (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? V11;
                    case 5: return (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? V11;
                    case 6: return (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? V11;
                    case 7: return (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? V11;
                    case 8: return (ISqlNode)V9 ?? (ISqlNode)V10 ?? V11;
                    case 9: return (ISqlNode)V10 ?? V11;
                    default: return V11;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3, V4, V5, V6, V7, V8, V9, V10, V11 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3, V4, V5, V6, V7, V8, V9, V10, V11 };
    }

    readonly struct SNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IReadOnlyList<ISqlNode>
            where T1 : ISqlNode
            where T2 : ISqlNode
            where T3 : ISqlNode
            where T4 : ISqlNode
            where T5 : ISqlNode
            where T6 : ISqlNode
            where T7 : ISqlNode
            where T8 : ISqlNode
            where T9 : ISqlNode
            where T10 : ISqlNode
            where T11 : ISqlNode
            where T12 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;
        public readonly T4 V4;
        public readonly T5 V5;
        public readonly T6 V6;
        public readonly T7 V7;
        public readonly T8 V8;
        public readonly T9 V9;
        public readonly T10 V10;
        public readonly T11 V11;
        public readonly T12 V12;

        public SNode( T1 o1, T2 o2, T3 o3, T4 o4, T5 o5, T6 o6, T7 o7, T8 o8, T9 o9, T10 o10, T11 o11, T12 o12 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
            if( (V4 = o4) != null ) ++Count;
            if( (V5 = o5) != null ) ++Count;
            if( (V6 = o6) != null ) ++Count;
            if( (V7 = o7) != null ) ++Count;
            if( (V8 = o8) != null ) ++Count;
            if( (V9 = o9) != null ) ++Count;
            if( (V10 = o10) != null ) ++Count;
            if( (V11 = o11) != null ) ++Count;
            if( (V12 = o12) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( x.MoveNext() )
                            {
                                if( (V4 = (T4)x.Current) != null ) ++Count;
                                if( x.MoveNext() )
                                {
                                    if( (V5 = (T5)x.Current) != null ) ++Count;
                                    if( x.MoveNext() )
                                    {
                                        if( (V6 = (T6)x.Current) != null ) ++Count;
                                        if( x.MoveNext() )
                                        {
                                            if( (V7 = (T7)x.Current) != null ) ++Count;
                                            if( x.MoveNext() )
                                            {
                                                if( (V8 = (T8)x.Current) != null ) ++Count;
                                                if( x.MoveNext() )
                                                {
                                                    if( (V9 = (T9)x.Current) != null ) ++Count;
                                                    if( x.MoveNext() )
                                                    {
                                                        if( (V10 = (T10)x.Current) != null ) ++Count;
                                                        if( x.MoveNext() )
                                                        {
                                                            if( (V11 = (T11)x.Current) != null ) ++Count;
                                                            if( x.MoveNext() )
                                                            {
                                                                if( (V12 = (T12)x.Current) != null ) ++Count;
                                                                if( !x.MoveNext() ) return;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}.",
                typeof( T1 ).Name,
                typeof( T2 ).Name,
                typeof( T3 ).Name,
                typeof( T4 ).Name,
                typeof( T5 ).Name,
                typeof( T6 ).Name,
                typeof( T7 ).Name,
                typeof( T8 ).Name,
                typeof( T9 ).Name,
                typeof( T10 ).Name,
                typeof( T11 ).Name,
                typeof( T12 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? V12;
                    case 1: return (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? V12;
                    case 2: return (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? V12;
                    case 3: return (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? V12;
                    case 4: return (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? V12;
                    case 5: return (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? V12;
                    case 6: return (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? V12;
                    case 7: return (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? V12;
                    case 8: return (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? V12;
                    case 9: return (ISqlNode)V10 ?? (ISqlNode)V11 ?? V12;
                    case 10: return (ISqlNode)V11 ?? V12;
                    default: return V12;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3, V4, V5, V6, V7, V8, V9, V10, V11, V12 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3, V4, V5, V6, V7, V8, V9, V10, V11, V12 };
    }

    readonly struct SNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IReadOnlyList<ISqlNode>
            where T1 : ISqlNode
            where T2 : ISqlNode
            where T3 : ISqlNode
            where T4 : ISqlNode
            where T5 : ISqlNode
            where T6 : ISqlNode
            where T7 : ISqlNode
            where T8 : ISqlNode
            where T9 : ISqlNode
            where T10 : ISqlNode
            where T11 : ISqlNode
            where T12 : ISqlNode
            where T13 : ISqlNode
    {
        public readonly int Count;
        public readonly T1 V1;
        public readonly T2 V2;
        public readonly T3 V3;
        public readonly T4 V4;
        public readonly T5 V5;
        public readonly T6 V6;
        public readonly T7 V7;
        public readonly T8 V8;
        public readonly T9 V9;
        public readonly T10 V10;
        public readonly T11 V11;
        public readonly T12 V12;
        public readonly T13 V13;

        public SNode( T1 o1, T2 o2, T3 o3, T4 o4, T5 o5, T6 o6, T7 o7, T8 o8, T9 o9, T10 o10, T11 o11, T12 o12, T13 o13 )
        {
            Count = (V1 = o1) != null ? 1 : 0;
            if( (V2 = o2) != null ) ++Count;
            if( (V3 = o3) != null ) ++Count;
            if( (V4 = o4) != null ) ++Count;
            if( (V5 = o5) != null ) ++Count;
            if( (V6 = o6) != null ) ++Count;
            if( (V7 = o7) != null ) ++Count;
            if( (V8 = o8) != null ) ++Count;
            if( (V9 = o9) != null ) ++Count;
            if( (V10 = o10) != null ) ++Count;
            if( (V11 = o11) != null ) ++Count;
            if( (V12 = o12) != null ) ++Count;
            if( (V13 = o13) != null ) ++Count;
        }

        public SNode( IEnumerable<ISqlNode> a )
        {
            using( var x = a.GetEnumerator() )
            {
                if( x.MoveNext() )
                {
                    Count = (V1 = (T1)x.Current) != null ? 1 : 0;
                    if( x.MoveNext() )
                    {
                        if( (V2 = (T2)x.Current) != null ) ++Count;
                        if( x.MoveNext() )
                        {
                            if( (V3 = (T3)x.Current) != null ) ++Count;
                            if( x.MoveNext() )
                            {
                                if( (V4 = (T4)x.Current) != null ) ++Count;
                                if( x.MoveNext() )
                                {
                                    if( (V5 = (T5)x.Current) != null ) ++Count;
                                    if( x.MoveNext() )
                                    {
                                        if( (V6 = (T6)x.Current) != null ) ++Count;
                                        if( x.MoveNext() )
                                        {
                                            if( (V7 = (T7)x.Current) != null ) ++Count;
                                            if( x.MoveNext() )
                                            {
                                                if( (V8 = (T8)x.Current) != null ) ++Count;
                                                if( x.MoveNext() )
                                                {
                                                    if( (V9 = (T9)x.Current) != null ) ++Count;
                                                    if( x.MoveNext() )
                                                    {
                                                        if( (V10 = (T10)x.Current) != null ) ++Count;
                                                        if( x.MoveNext() )
                                                        {
                                                            if( (V11 = (T11)x.Current) != null ) ++Count;
                                                            if( x.MoveNext() )
                                                            {
                                                                if( (V12 = (T12)x.Current) != null ) ++Count;
                                                                if( x.MoveNext() )
                                                                {
                                                                    if( (V13 = (T13)x.Current) != null ) ++Count;
                                                                    if( !x.MoveNext() ) return;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException( string.Format( "Expected {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}.",
                typeof( T1 ).Name,
                typeof( T2 ).Name,
                typeof( T3 ).Name,
                typeof( T4 ).Name,
                typeof( T5 ).Name,
                typeof( T6 ).Name,
                typeof( T7 ).Name,
                typeof( T8 ).Name,
                typeof( T9 ).Name,
                typeof( T10 ).Name,
                typeof( T11 ).Name,
                typeof( T12 ).Name,
                typeof( T13 ).Name ), "content" );
        }

        public ISqlNode this[int index]
        {
            get
            {
                if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
                switch( index )
                {
                    case 0: return (ISqlNode)V1 ?? (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 1: return (ISqlNode)V2 ?? (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 2: return (ISqlNode)V3 ?? (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 3: return (ISqlNode)V4 ?? (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 4: return (ISqlNode)V5 ?? (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 5: return (ISqlNode)V6 ?? (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 6: return (ISqlNode)V7 ?? (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 7: return (ISqlNode)V8 ?? (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 8: return (ISqlNode)V9 ?? (ISqlNode)V10 ?? (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 9: return (ISqlNode)V10 ?? (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 10: return (ISqlNode)V11 ?? (ISqlNode)V12 ?? V13;
                    case 11: return (ISqlNode)V12 ?? V13;
                    default: return V13;
                }
            }
        }

        int IReadOnlyCollection<ISqlNode>.Count => Count;

        public IEnumerator<ISqlNode> GetEnumerator() => SNode.CreateFilteredEnumerator( V1, V2, V3, V4, V5, V6, V7, V8, V9, V10, V11, V12, V13 );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<ISqlNode> GetRawContent() => new ISqlNode[] { V1, V2, V3, V4, V5, V6, V7, V8, V9, V10, V11, V12, V13 };

    }

}
