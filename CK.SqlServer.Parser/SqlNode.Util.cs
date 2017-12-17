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
    public abstract partial class SqlNode
    {
        static internal class Helper
        {
            public static T[] EnsureArray<T>( IEnumerable<T> content )
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


            public static void CheckIsVariable( SqlTokenIdentifier token, string name )
            {
                if( token == null ) throw new ArgumentNullException( name );
                if( !token.IsVariable ) throw new ArgumentException( "Must be a @VariableName", name );
            }

            public static void CheckIsVariable( SqlTypedIdentifier identifier, string name )
            {
                if( identifier == null ) throw new ArgumentNullException( name );
                if( !identifier.Identifier.IsVariable ) throw new ArgumentException( "Must be a @VariableName", name );
            }

            public static void CheckToken( SqlToken token, string name, SqlTokenType tokenType )
            {
                if( token == null ) throw new ArgumentNullException( name );
                if( token.TokenType != tokenType )
                {
                    throw new ArgumentException( string.Format( "{0} must be {1}, not {2}.",
                                                                name,
                                                                SqlKeyword.ToString( tokenType ),
                                                                token.ToString() ), name );
                }
            }

            public static void CheckToken( SqlToken token, string name, Func<SqlTokenType, bool> predicate )
            {
                if( token == null ) throw new ArgumentNullException( name );
                if( !predicate( token.TokenType ) )
                {
                    throw new ArgumentException( $"'{name}' must satisfy '{predicate}' predicate.", name );
                }
            }

            public static void CheckNullableToken( SqlToken token, string name, SqlTokenType tokenType )
            {
                if( token != null && token.TokenType != tokenType )
                {
                    throw new ArgumentException( string.Format( "Optional {0} must be {1}, not {2}.",
                                                                name,
                                                                SqlKeyword.ToString( tokenType ),
                                                                SqlKeyword.ToString( token.TokenType ) ), name );
                }
            }

            public static void CheckNullableToken( SqlToken token, string name, SqlTokenType t1, SqlTokenType t2 )
            {
                if( token != null && token.TokenType != t1 && token.TokenType != t2 )
                {
                    throw new ArgumentException( string.Format( "Optional {0} must be {1} or {2}, not {3}.",
                                                                name,
                                                                SqlKeyword.ToString( t1 ),
                                                                SqlKeyword.ToString( t2 ),
                                                                SqlKeyword.ToString( token.TokenType ) ), name );
                }
            }

            public static void CheckNullableToken( SqlToken token, string name, SqlTokenType t1, SqlTokenType t2, SqlTokenType t3 )
            {
                if( token != null && token.TokenType != t1 && token.TokenType != t2 && token.TokenType != t3 )
                {
                    throw new ArgumentException( string.Format( "{0} must be {1}, {2} or {3}, not {4}.",
                                                                name,
                                                                SqlKeyword.ToString( t1 ),
                                                                SqlKeyword.ToString( t2 ),
                                                                SqlKeyword.ToString( t3 ),
                                                                SqlKeyword.ToString( token.TokenType ) ), name );
                }
            }

            public static void CheckNullableToken( SqlToken token, string name, SqlTokenType t1, SqlTokenType t2, SqlTokenType t3, SqlTokenType t4 )
            {
                if( token != null && token.TokenType != t1 && token.TokenType != t2 && token.TokenType != t3 && token.TokenType != t4 )
                {
                    throw new ArgumentException( string.Format( "{0} must be {1}, {2}, {3} or {4}, not {5}.",
                                                                name,
                                                                SqlKeyword.ToString( t1 ),
                                                                SqlKeyword.ToString( t2 ),
                                                                SqlKeyword.ToString( t3 ),
                                                                SqlKeyword.ToString( t4 ),
                                                                SqlKeyword.ToString( token.TokenType ) ), name );
                }
            }

            public static void CheckNullableToken( SqlToken token, string name, Func<SqlTokenType, bool> predicate )
            {
                if( token == null && !predicate( token.TokenType ) )
                {
                    throw new ArgumentException( $"{name} must satisfy '{predicate}' predicate.", name );
                }
            }

            internal static void CheckBothNullOrNot( ISqlNode e1, string n1, ISqlNode e2, string n2 )
            {
                if( (e1 != null) != (e2 != null) )
                {
                    throw new ArgumentException( String.Format( "{0} and {1} must be both null or not null.", n1, n2 ) );
                }
            }

            internal static void CheckAllNullOrNot( ISqlNode e1, string n1, ISqlNode e2, string n2, ISqlNode e3, string n3 )
            {
                if( (e1 != null) != (e2 != null) || (e1 != null) != (e3 != null) )
                {
                    throw new ArgumentException( $"{n1}, {n2} and {n3} must all be null or not null." );
                }
            }

            internal static void CheckXORNull( ISqlNode e1, string n1, ISqlNode e2, string n2 )
            {
                if( (e1 != null) == (e2 != null) )
                {
                    throw new ArgumentException( String.Format( "Either {0} or {1} must be defined and not both.", n1, n2 ) );
                }
            }

            /// <summary>
            /// Checks that a token's type is one of the two expected.
            /// Throws an <see cref="ArgumentException"/> if it is not the case.
            /// </summary>
            /// <param name="token">The token to check. Must not be null.</param>
            /// <param name="name">The name of the token (used for exception message).</param>
            /// <param name="t1">First possible token type.</param>
            /// <param name="t2">Second possible token type.</param>
            public static void CheckToken( SqlToken token, string name, SqlTokenType t1, SqlTokenType t2 )
            {
                if( token == null ) throw new ArgumentNullException( name );
                if( token.TokenType != t1 && token.TokenType != t2 )
                {
                    throw new ArgumentException( $"{name} must be {SqlKeyword.ToString( t1 )} or {SqlKeyword.ToString( t2 )}, not {SqlKeyword.ToString( token.TokenType )}." );
                }
            }

            public static void CheckToken( SqlToken token, string name, SqlTokenType t1, SqlTokenType t2, SqlTokenType t3 )
            {
                if( token == null ) throw new ArgumentNullException( name );
                if( token.TokenType != t1 && token.TokenType != t2 && token.TokenType != t3 )
                {
                    throw new ArgumentException( string.Format( "{0} must be {1}, {2} or {3}, not {4}.",
                                                                name,
                                                                SqlKeyword.ToString( t1 ),
                                                                SqlKeyword.ToString( t2 ),
                                                                SqlKeyword.ToString( t3 ),
                                                                SqlKeyword.ToString( token.TokenType ) ), name );
                }
            }

            public static void CheckToken( SqlToken token, string name, SqlTokenType t1, SqlTokenType t2, SqlTokenType t3, SqlTokenType t4 )
            {
                if( token == null ) throw new ArgumentNullException( name );
                if( token.TokenType != t1 && token.TokenType != t2 && token.TokenType != t3 && token.TokenType != t4 )
                {
                    throw new ArgumentException( string.Format( "{0} must be {1}, {2}, {3} or {4}, not {5}.",
                                                                name,
                                                                SqlKeyword.ToString( t1 ),
                                                                SqlKeyword.ToString( t2 ),
                                                                SqlKeyword.ToString( t3 ),
                                                                SqlKeyword.ToString( t4 ),
                                                                SqlKeyword.ToString( token.TokenType ) ), name );
                }
            }

            public static void CheckToken( SqlToken token, string name, SqlTokenType t1, SqlTokenType t2, SqlTokenType t3, SqlTokenType t4, SqlTokenType t5 )
            {
                if( token == null ) throw new ArgumentNullException( name );
                if( token.TokenType != t1 && token.TokenType != t2 && token.TokenType != t3 && token.TokenType != t4 && token.TokenType != t5 )
                {
                    throw new ArgumentException( string.Format( "{0} must be {1}, {2}, {3}, {4} or {5}, not {6}.",
                                                                name,
                                                                SqlKeyword.ToString( t1 ),
                                                                SqlKeyword.ToString( t2 ),
                                                                SqlKeyword.ToString( t3 ),
                                                                SqlKeyword.ToString( t4 ),
                                                                SqlKeyword.ToString( t5 ),
                                                                SqlKeyword.ToString( token.TokenType ) ), name );
                }
            }

            public static void CheckNotNull( ISqlNode e, string name )
            {
                if( e == null ) throw new ArgumentNullException( name );
            }

            public static void CheckNotNull<T>( ISqlNode e, string name )
            {
                if( e == null ) throw new ArgumentNullException( name );
                if( !(e is T) ) throw new ArgumentException( $"Must be a '{typeof( T ).Name}'.", name );
            }

            public static void CheckNotNull<T1,T2>( ISqlNode e, string name )
            {
                if( e == null ) throw new ArgumentNullException( name );
                if( !(e is T1) && !(e is T2) )
                {
                    throw new ArgumentException( $"Must be a '{typeof( T1 ).Name}' or '{typeof( T2 ).Name}'.", name );
                }
            }

            public static void CheckUnPar<T>( ISqlNode e, string name )
            {
                if( e == null ) throw new ArgumentNullException( name );
                if( !(e.UnPar is T) ) throw new ArgumentException( String.Format( "Must be '{0}' but found '{1}'.", typeof( T ).Name, e.GetType().Name ), name );
            }

            public static void CheckNull( ISqlNode e, string name )
            {
                if( e != null ) throw new ArgumentException( "Must be null.", name );
            }

        }
    }
}
