using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures 'CURSOR' or special 'CURSOR VARYING' parameter type. 
    /// </summary>
    public sealed class SqlTypeDeclCursorParameter : SqlNode, ISqlUnifiedTypeDecl
    {
        readonly SNode<SqlTokenIdentifier, SqlTokenIdentifier> _content;

        public SqlTypeDeclCursorParameter( SqlTokenIdentifier cursorT, SqlTokenIdentifier varyingT )
            : base( null, null )
        {
            _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier>( cursorT, varyingT );
        }

        void CheckContent()
        {
            Helper.CheckToken( CursorT, nameof( CursorT ), SqlTokenType.Cursor );
            Helper.CheckNullableToken( VaryingT, nameof( VaryingT ), SqlTokenType.Varying );
        }

        SqlTypeDeclCursorParameter( SqlTypeDeclCursorParameter o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null ) _content = o._content;
            else
            {
                _content = new SNode<SqlTokenIdentifier, SqlTokenIdentifier>( items );
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SqlTypeDeclCursorParameter( this, leading, children, trailing );
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _content;

        public override IList<ISqlNode> GetRawContent() => _content.GetRawContent();

        /// <summary>
        /// Gets the <see cref="SqlDbType.Udt"/>. 
        /// By using Udt here, we avoid the introduction of a new invalid, undefined <see cref="SqlDbType"/>.
        /// </summary>
        public SqlDbType DbType => SqlDbType.Udt;

        public SqlTokenIdentifier CursorT => _content.V1;

        /// <summary>
        /// Gets VARYING identifier (can appear only in a procedure parameter).
        /// </summary>
        public SqlTokenIdentifier VaryingT => _content.V2;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

        string ISqlServerUnifiedTypeDecl.ToStringClean() => ChildrenNodes.ToStringCompact();

        int ISqlServerUnifiedTypeDecl.SyntaxSize => -2;

        byte ISqlServerUnifiedTypeDecl.SyntaxPrecision => 0;

        byte ISqlServerUnifiedTypeDecl.SyntaxScale => 0;

        int ISqlServerUnifiedTypeDecl.SyntaxSecondScale => 1;
    }

}
