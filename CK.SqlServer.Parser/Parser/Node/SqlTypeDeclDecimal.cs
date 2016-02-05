using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CK.Core;
using System.Collections.Immutable;

namespace CK.SqlServer.Parser
{
    public sealed class SqlTypeDeclDecimal : SqlNode, ISqlUnifiedTypeDecl
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenComma, SqlTokenLiteralInteger, SqlTokenClosePar> _content;

        public SqlTypeDeclDecimal( SqlTokenIdentifier id )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenComma, SqlTokenLiteralInteger, SqlTokenClosePar>( 
                id, null, null, null, null, null );
            CheckContent();
        }

        void CheckContent()
        {
            Helper.CheckToken( TypeIdentifierT, nameof( TypeIdentifierT ), SqlTokenType.DecimalDbType );
            if( _content.Count > 1 )
            {
                Helper.CheckNotNull( Opener, nameof( Opener ) );
                Helper.CheckNotNull( Precision, nameof( Precision ) );
                if( Precision.Value <= 0 || Precision.Value > 38 )
                    throw new ArgumentException( "Invalid precision.", nameof( Precision ) );
                if( _content.Count > 4 )
                {
                    Helper.CheckNotNull( Comma, nameof( Comma ) );
                    Helper.CheckNotNull( Scale, nameof( Scale ) );
                    if( Scale.Value < 0 || Scale.Value > Precision.Value )
                        throw new ArgumentException( "Invalid scale (must be less or equal to precision).", nameof( Scale ) );
                }
                Helper.CheckNotNull( Closer, nameof( Closer ) );
            }
        }

        public SqlTypeDeclDecimal( SqlTokenIdentifier id, SqlTokenOpenPar openPar, SqlTokenLiteralInteger precision, SqlTokenClosePar closePar )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenComma, SqlTokenLiteralInteger, SqlTokenClosePar>(
                id, openPar, precision, null, null, closePar );
            CheckContent();
        }

        public SqlTypeDeclDecimal( SqlTokenIdentifier id, SqlTokenOpenPar openPar, SqlTokenLiteralInteger precision, SqlTokenComma comma, SqlTokenLiteralInteger scale, SqlTokenClosePar closePar )
             : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenComma, SqlTokenLiteralInteger, SqlTokenClosePar>(
                id, openPar, precision, comma, scale, closePar );
            CheckContent();
        }

        SqlTypeDeclDecimal( SqlTypeDeclDecimal o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenOpenPar, SqlTokenLiteralInteger, SqlTokenComma, SqlTokenLiteralInteger, SqlTokenClosePar>( items );
                CheckContent();
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTypeDeclDecimal( this, leading, content, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        public SqlDbType DbType => SqlDbType.Decimal;

        public SqlTokenIdentifier TypeIdentifierT => _content.V1;

        public SqlTokenTerminal Opener => _content.V2;

        public SqlTokenLiteralInteger Precision => _content.V3;

        public SqlTokenTerminal Comma => _content.V4;

        public SqlTokenLiteralInteger Scale => _content.V5;

        public SqlTokenTerminal Closer => _content.V6;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

        byte ISqlServerUnifiedTypeDecl.SyntaxPrecision => Precision != null ? (byte)Precision.Value : (byte)0;

        byte ISqlServerUnifiedTypeDecl.SyntaxScale => Scale != null ? (byte)Scale.Value : (byte)0;

        int ISqlServerUnifiedTypeDecl.SyntaxSize => -2;

        int ISqlServerUnifiedTypeDecl.SyntaxSecondScale => -1;

    }

}
