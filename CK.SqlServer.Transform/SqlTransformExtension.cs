using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Transform
{
    public static class SqlTransformExtension
    {
        public static T InsertParameter<T>( this T @this, int idx, SqlParameter parameter ) where T : ISqlServerCallableObject
        {
            ISqlParameterListHolder h = @this as ISqlParameterListHolder;
            if( h == null ) throw new ArgumentException( "Must be a ISqlParameterListHolder." );
            return (T)h.InsertParameter( idx, parameter );
        }

        public static T InsertParameter<T>( this T @this, SqlParameter parameter, string paramNameBefore = null, string paramNameAfter = null ) where T : ISqlServerCallableObject
        {
            ISqlParameterListHolder h = @this as ISqlParameterListHolder;
            if( h == null ) throw new ArgumentException( "Must be a ISqlParameterListHolder." );
            return (T)h.InsertParameter( parameter, paramNameBefore, paramNameAfter );
        }

        public static ISqlParameterListHolder InsertParameter( this ISqlParameterListHolder @this, int idx, SqlParameter parameter )
        {
            return @this.SetParameters( @this.Parameters.InsertAt( idx, parameter ) );
        }

        public static ISqlParameterListHolder InsertParameter( this ISqlParameterListHolder @this, SqlParameter parameter, string paramNameBefore = null, string paramNameAfter = null )
        {
            return InsertParameter( @this, @this.Parameters.GetInsertIndex( paramNameBefore, paramNameAfter ), parameter );
        }

        public static ISqlNodeLocationRange Intersect( this ISqlNodeLocationRange @this, ISqlNodeLocationRange other )
        {
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            return SqlNodeScopeIntersect.DoIntersect( @this, other );
        }

        public static ISqlNodeLocationRange Union( this ISqlNodeLocationRange @this, ISqlNodeLocationRange other )
        {
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            return SqlNodeScopeUnion.DoUnion( @this, other );
        }

        public static ISqlNodeLocationRange Except( this ISqlNodeLocationRange @this, ISqlNodeLocationRange other )
        {
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            return SqlNodeScopeExcept.DoExcept( @this, other );
        }

        public static IEnumerable<SqlNodeLocationRange> MergeContiguous( this IEnumerable<SqlNodeLocationRange> @this )
        {
            using( var e = @this.GetEnumerator() )
            {
                bool hasNext = e.MoveNext();
                while( hasNext )
                {
                    SqlNodeLocationRange current = e.Current;
                    hasNext = e.MoveNext();
                    while( hasNext && current.End.Position == e.Current.Beg.Position )
                    {
                        current = current.InternalSetEnd( e.Current.End );
                        hasNext = e.MoveNext();
                    }
                    yield return current;
                }
            }
        }

    }
}
