using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// A SelectDecorator operator applies to a <see cref="Select"/> and decorates it
    /// with a <see cref="OrderBy"/> clause, a <see cref="For"/> xml, browse, json or system_time, 
    /// and/or a <see cref="Option"/> clause (int this order).
    /// </summary>
    public class SelectDecorator : SqlNonToken, ISelectSpecification, ISqlStatementPart
    {
        readonly SNode<ISqlNode, SelectOrderBy, SelectFor, SqlOptionParOptions> _content;

        public SelectDecorator( 
            ISqlNode selectNode, 
            SelectOrderBy orderBy,
            SelectFor forClause,
            SqlOptionParOptions option )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SelectOrderBy, SelectFor, SqlOptionParOptions>(
                selectNode, 
                orderBy,
                forClause,
                option );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckUnPar<ISelectSpecification>( SelectNode, nameof( SelectNode ) );
        }

        SelectDecorator( SelectDecorator o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SelectOrderBy, SelectFor, SqlOptionParOptions>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectDecorator( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public ISqlNode SelectNode => _content.V1;

        public ISelectSpecification Select => (ISelectSpecification)_content.V1.UnPar;

        public bool HasOrderBy => _content.V2 != null;

        public SelectOrderBy OrderBy => _content.V2;

        public bool HasFor => _content.V3 != null;

        public SelectFor For => _content.V3;

        public bool HasOption => _content.V4 != null;

        public SqlOptionParOptions Option => _content.V4;

        SelectOperatorKind ISelectSpecification.SelectOperator => SelectOperatorKind.Decorator; 

        SelectColumnList ISelectSpecification.Columns => Select.Columns; 

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
