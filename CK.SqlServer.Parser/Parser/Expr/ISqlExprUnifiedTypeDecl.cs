#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\ISqlExprUnifiedTypeDecl.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Unifies Sql types <see cref="SqlExprTypeDeclDateAndTime"/>, <see cref="SqlExprTypeDeclDecimal"/>, <see cref="SqlExprTypeDeclSimple"/>, <see cref="SqlExprTypeDeclWithSize"/>
    /// and <see cref="SqlExprTypeDeclUserDefined"/>.
    /// </summary>
    /// <remarks>
    /// This is not an attempt to model the actual type capacity, but only focuses on syntax representation. <see cref="SqlDbType.DateTime"/> for example
    /// has a Precision of 23 and a Scale of 3 in terms of digits, but here, we consider Precision and Scale to be 0 (non applicable) since 'datetime(p,s)' is not valid.
    /// To make this more explicit, the Size/Precision/Scale properties have been prefixed with 'Syntax'.
    /// </remarks>
    public interface ISqlExprUnifiedTypeDecl : ISqlItem, ISqlServerUnifiedTypeDecl
    {
    }
}
