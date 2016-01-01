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
    /// Select "For" operator: handles 'for browse', 'for xml', 'for JSON' and 'for SYSTEM_TIME'.
    /// </summary>
    public sealed class SelectFor : SqlNode, ISelectSpecification
    {
        readonly SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode> _content;

        public SelectFor( ISqlNode selectNode, SqlTokenIdentifier forToken, SqlTokenIdentifier targetType, ISqlNode forExpression )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode>( selectNode, forToken, targetType,  forExpression );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckUnPar<ISelectSpecification>( SelectNode, nameof( SelectNode ) );
            SNode.CheckToken( ForT, nameof( ForT ), SqlTokenType.For );
            SNode.CheckToken( TargeType, nameof( TargeType ), 
                SqlTokenType.XmlDbType, 
                SqlTokenType.Browse, 
                SqlTokenType.Json, 
                SqlTokenType.SystemTime );
            SNode.CheckNotNull( ForExpression, nameof( ForExpression ) );
        }

        SelectFor( SelectFor o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenIdentifier, ISqlNode>( items );
                CheckContent();
            }

        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectFor( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlNode SelectNode => _content.V1;

        public ISelectSpecification Select => (ISelectSpecification)_content.V1.UnPar;

        public SqlTokenIdentifier ForT => _content.V2;

        public SqlTokenIdentifier TargeType => _content.V3;

        public ISqlNode ForExpression => _content.V4;

        /// <summary>
        /// Gets the operator: either <see cref="SelectOperatorKind.ForXml"/>, <see cref="SelectOperatorKind.ForBrowse"/>,
        /// <see cref="SelectOperatorKind.ForJSON"/> or <see cref="SelectOperatorKind.ForSystemTime"/>.
        /// </summary>
        public SelectOperatorKind SelectOperator
        {
            get
            {
                switch( TargeType.TokenType )
                {
                    case SqlTokenType.XmlDbType: return SelectOperatorKind.ForXml;
                    case SqlTokenType.Browse: return SelectOperatorKind.ForBrowse;
                    case SqlTokenType.Json: return SelectOperatorKind.ForJSON;
                    default: return SelectOperatorKind.ForSystemTime;
                }
            }
        }

        public SelectColumnList Columns => Select.Columns; 

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
