using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{

    /// <summary>
    /// Generic IsXXX matcher function.
    /// </summary>
    /// <typeparam name="T">Type to match.</typeparam>
    /// <param name="e">Not null when matched.</param>
    /// <param name="expected">True to set an error if not found.</param>
    /// <returns>True if a <typeparamref name="T"/> has been matched, false otherwise.</returns>
    public delegate bool IsFunc<T>( out T e, bool expected );

    static class IsFuncExtension
    {

        public static IsFunc<T> Or<T>( this IsFunc<T> @this, IsFunc<T> x ) where T : class, ISqlNode 
        {
            return delegate ( out T e, bool expected )
            {
                return @this( out e, false ) ? true : x( out e, false );
            };
        }

        public static IsFunc<ISqlNode> AsNode<T>( this IsFunc<T> @this )
            where T : class, ISqlNode
        {
            return delegate ( out ISqlNode e, bool expected )
            {
                e = null;
                T n;
                if( !@this( out n, expected ) ) return false;
                e = n;
                return true;
            };
        }

    }
}
