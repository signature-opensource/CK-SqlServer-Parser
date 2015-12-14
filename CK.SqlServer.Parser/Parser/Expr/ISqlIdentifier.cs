#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\ISqlIdentifier.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Unifies <see cref="SqlExprIdentifier"/> and <see cref="SqlExprMultiIdentifier"/>.
    /// </summary>
    public interface ISqlIdentifier
    {

        IEnumerable<SqlTrivia> FullLeadingTrivias { get; }

        /// <summary>
        /// Gets the number of <see cref="Identifiers"/>.
        /// </summary>
        int IdentifiersCount { get; }

        /// <summary>
        /// Gets the <see cref="SqlTokenIdentifier"/> (without the separators).
        /// </summary>
        IEnumerable<SqlTokenIdentifier> Identifiers { get; }

        /// <summary>
        /// Gets the <see cref="SqlTokenIdentifier"/> (without the separators).
        /// </summary>
        SqlTokenIdentifier IdentifierAt( int i );

        /// <summary>
        /// Gets whether this identifier is a variable.
        /// </summary>
        bool IsVariable { get; }

        /// <summary>
        /// Gets the sql items (<see cref="SqlTokenIdentifier"/> and <see cref="SqlTokenTerminal"/> for the separators) without 
        /// the enclosing parenthesis if they exist.
        /// </summary>
        IEnumerable<SqlToken> TokensWithoutParenthesis { get; }

    }
}
