using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
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

    }
}
