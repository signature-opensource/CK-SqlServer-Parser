using CK.Core;
using CK.SqlServer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Transform.Transformers
{
    public class AddParameter : SqlNodeLocationVisitor
    {
        readonly IEnumerable<SqlParameter> _param;
        readonly string _paramNameBefore;
        readonly string _paramNameAfter;

        /// <summary>
        /// Initializes a new <see cref="AddParameter"/> visitor that can insert one or more parameters
        /// into any <see cref="SqlParameterList"/>.
        /// Note that <paramref name="paramNameBefore"/> is the parameter that WILL BE BEFORE the inserted parameters 
        /// (and <paramref name="paramNameAfter"/> WILL APPEAR AFTER the inserted parameters).
        /// When both <paramref name="paramNameBefore"/> and <paramref name="paramNameAfter"/> are defined,
        /// the latter is ignored.
        /// </summary>
        /// <param name="param">The parameters to insert.</param>
        /// <param name="paramNameBefore">Optional name of the parameter that will be before the new ones.</param>
        /// <param name="paramNameAfter">Optional name of the parameter that will be after the new ones.</param>
        public AddParameter( IEnumerable<SqlParameter> param, string paramNameBefore = null, string paramNameAfter = null )
        {
            _param = param;
            _paramNameBefore = paramNameBefore;
            _paramNameAfter = paramNameAfter;
        }

        protected override ISqlNode Visit( SqlParameterList e )
        {
            int idx = _paramNameBefore != null 
                        ? e.IndexOf( p => p.Name == _paramNameBefore ) + 1 
                        : (_paramNameAfter != null 
                            ? e.IndexOf( p => p.Name == _paramNameAfter ) 
                            : e.Count);
            StopVisit();
            foreach( var p in _param )
            {
                e = e.InsertAt( idx++, p );
            }
            return e;
        }
    }
}
