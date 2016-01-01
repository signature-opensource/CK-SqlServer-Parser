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
    /// Select Options operator.
    /// </summary>
    public sealed class SelectOption : SqlNode, ISelectSpecification
    {
        readonly SNode<ISqlNode, SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar> _content;

        public SelectOption( ISqlNode selectNode, SqlTokenIdentifier optionsT, SqlTokenOpenPar opener, ISqlNode content, SqlTokenClosePar closer )
            : base( null, null )
        {
            _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>( selectNode, optionsT, opener, content, closer );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckUnPar<ISelectSpecification>( SelectNode, nameof( SelectNode ) );
            SNode.CheckToken( OptionsT, nameof( OptionsT ), SqlTokenType.Option );
            SNode.CheckNotNull( Opener, nameof( Opener ) );
            SNode.CheckNotNull( Content, nameof( Content ) );
            SNode.CheckNotNull( Closer, nameof( Closer ) );
        }

        SelectOption( SelectOption o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<ISqlNode, SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectOption( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public ISqlNode SelectNode => _content.V1;

        public ISelectSpecification Select => (ISelectSpecification)_content.V1.UnPar;

        public SqlTokenIdentifier OptionsT => _content.V2;

        public SqlTokenOpenPar Opener => _content.V3;

        public ISqlNode Content => _content.V4;

        public SqlTokenClosePar Closer => _content.V5;

        SelectOperatorKind ISelectSpecification.SelectOperator => SelectOperatorKind.Option;

        public SelectColumnList Columns => Select.Columns; 

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
