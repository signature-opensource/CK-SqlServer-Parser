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
    /// Defines "next value for {sequence}>" expression.
    /// </summary>
    public sealed class SqlCTEName : SqlNode
    {
        readonly SNode<
            SqlTokenIdentifier,
            SqlEnclosedIdentifierCommaList, 
            SqlTokenIdentifier,
            SqlTokenOpenPar,
            ISqlNode,
            SqlTokenClosePar> _content;

        public SqlCTEName( 
                SqlTokenIdentifier name,
                SqlEnclosedIdentifierCommaList optionalColumnNames, 
                SqlTokenIdentifier asT,
                SqlTokenOpenPar opener,
                ISqlNode selectNode,
                SqlTokenClosePar closer )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlEnclosedIdentifierCommaList, SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>( 
                name, optionalColumnNames, asT, opener, selectNode, closer );
            CheckContent();
        }

        void CheckContent()
        {
            SNode.CheckNotNull( Name, nameof( Name ) );
            SNode.CheckToken( AsT, nameof( AsT ), SqlTokenType.As );
            SNode.CheckNotNull( Opener, nameof( Opener ) );
            SNode.CheckUnPar<ISelectSpecification>( SelectNode, nameof( SelectNode ) );
            SNode.CheckNotNull( Closer, nameof( Closer ) );
        }

        SqlCTEName( SqlCTEName o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlEnclosedIdentifierCommaList, SqlTokenIdentifier, SqlTokenOpenPar, ISqlNode, SqlTokenClosePar>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlCTEName( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public SqlTokenIdentifier Name => _content.V1;

        public bool HasColumnNames => _content.V2 != null;

        public SqlEnclosedIdentifierCommaList ColumnNames => _content.V2;

        public SqlTokenIdentifier AsT => _content.V3;

        public SqlTokenOpenPar Opener => _content.V4;

        public ISqlNode SelectNode => _content.V5;

        public ISelectSpecification Select => (ISelectSpecification)_content.V5.UnPar;

        public SqlTokenClosePar Closer => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
