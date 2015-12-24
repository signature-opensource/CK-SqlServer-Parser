#region Proprietary License
/*----------------------------------------------------------------------------
* This file (CK.SqlServer.Parser\Parser\Expr\Select\SelectQuery.cs) is part of CK-Database. 
* Copyright © 2007-2014, Invenietis <http://www.invenietis.com>. All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures a select specification (a <see cref="ISelectSpecification"/> object) followed by 
    /// optional <see cref="SelectOrderBy"/>, <see cref="SelectFor"/> and <see cref="SelectOption"/> clauses.
    /// </summary>
    public class SelectQuery : SqlNode
    {
        readonly SNode<ISelectSpecification, SelectOrderBy, SelectFor, SelectOption> _content;

        public SelectQuery( ISelectSpecification spec, SelectOrderBy orderBy = null, SelectFor forPart = null, SelectOption option = null )
            : base( null, null )
        {
            _content = new SNode<ISelectSpecification, SelectOrderBy, SelectFor, SelectOption>( spec, orderBy, forPart, option );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Specification, nameof( Specification ) );
        }


        SelectQuery( SelectQuery o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISelectSpecification, SelectOrderBy, SelectFor, SelectOption>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectQuery( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISelectSpecification Specification => _content.V1;

        public SelectOrderBy Orderby => _content.V2;

        public SelectFor ForPart => _content.V3;

        public SelectOption Option => _content.V4;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }
}
