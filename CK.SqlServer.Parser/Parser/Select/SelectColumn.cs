using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures a select column definition: it is either 'definition as name', 'name = definition' or the definition alone.
    /// The horrible syntax 'definition name' is also supported.
    /// </summary>
    public sealed class SelectColumn : SqlNode
    {
        readonly ISqlNode[] _items;
        readonly SqlToken _colName;
        readonly SqlToken _asOrEqual;
        readonly ISqlNode _definition;

        static readonly SqlTokenIdentifier _autoAsT = new SqlTokenIdentifier( SqlTokenType.As, "as", SqlTrivia.OneSpace, SqlTrivia.OneSpace );
        static readonly SqlTokenIdentifier _autoAsTNoLeft = new SqlTokenIdentifier( SqlTokenType.As, "as", null, SqlTrivia.OneSpace );
        static readonly SqlTokenIdentifier _autoAsTNoRight = new SqlTokenIdentifier( SqlTokenType.As, "as", SqlTrivia.OneSpace, null );
        static readonly SqlTokenIdentifier _autoAsTNoSpace = new SqlTokenIdentifier( SqlTokenType.As, "as", null, null );
        static readonly SqlTokenTerminal _autoAssignTNoSpace = SqlTokenTerminal.Create( SqlTokenType.Assign, null, null );

        public SelectColumn( SqlToken colName, SqlTokenTerminal assignT, ISqlNode definition )
            : this( null, null, new[] { colName, assignT, definition }, null )
        {
        }

        public SelectColumn( ISqlNode definition, SqlTokenIdentifier asT, SqlToken colName )
            : this( null, null, new[] { definition, asT, colName }, null )
        {
        }

        public SelectColumn( ISqlNode definition, SqlToken colName )
            : this( null, null, new[] { definition, colName }, null )
        {
        }

        public SelectColumn( ISqlNode definition )
            : this( null, null, new[] { definition }, null )
        {
        }

        public override IReadOnlyList<ISqlNode> ChildrenNodes => _items;

        /// <summary>
        /// Gets the mutable content: it is a <see cref="List{T}"/> of non null ISqlNode.
        /// </summary>
        /// <returns><see cref="List{T}"/> of non null ISqlNode.</returns>
        public override IList<ISqlNode> GetRawContent() => _items.ToList();

        SelectColumn( SelectColumn o, ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, trailing )
        {
            if( items == null )
            {
                _items = o._items;
                _definition = o._definition;
                _asOrEqual = o._asOrEqual;
                _colName = o._colName;
            }
            else
            {
                _items = items;
                if( items.Length == 1 ) _definition = items[0];
                else
                {
                    if( items.Length == 2 )
                    {
                        _definition = items[0];
                        _colName = items[1] as SqlToken;
                        Helper.CheckToken( ColumnName, nameof( ColumnName ), SqlTokenTypeExtension.IsValidColumnAliasName );
                    }
                    else
                    {
                        if( items.Length == 0 || items.Length > 3 )
                        {
                            throw new ArgumentException( "Between 1 and 3 parts must be provided." );
                        }
                        _asOrEqual = _items[1] as SqlToken;
                        if( _asOrEqual is SqlTokenTerminal )
                        {
                            _colName = (SqlToken)_items[0];
                            Helper.CheckToken( AsOrEqualT, nameof( AsOrEqualT ), SqlTokenType.Assign );
                            _definition = _items[2];
                            Helper.CheckToken( ColumnName, nameof( ColumnName ), SqlTokenTypeExtension.IsValidColumnAliasNameOrVariable );
                        }
                        else
                        {
                            Helper.CheckToken( AsOrEqualT, nameof( AsOrEqualT ), SqlTokenType.As );
                            _colName = _items[2] as SqlToken;
                            _definition = _items[0];
                            Helper.CheckToken( ColumnName, nameof( ColumnName ), SqlTokenTypeExtension.IsValidColumnAliasName );
                        }
                    }
                }
                Helper.CheckNotNull( Definition, nameof( Definition ) );
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectColumn( this, leading, content == null ? null : content.Where( n => n != null ).ToArray(), trailing );
        }

        /// <summary>
        /// Returns a column selector that uses the "as" syntax if possible: 'select 1 as @i' is not 
        /// valid (only the equal syntax is supported with variables). In such case, this SelectColumn 
        /// is returned as-is.
        /// </summary>
        /// <returns>A column selector with the "as" syntax if possible.</returns>
        public SelectColumn ToAsSyntax()
        {
            if( _colName == null || IsAsSyntax || _colName.TokenType == SqlTokenType.IdentifierVariable ) return this;
            if( IsHorribleSyntax )
            {
                return new SelectColumn( null, LeadingTrivias, new[] { _definition, _autoAsTNoSpace, _colName }, TrailingTrivias );
            }
            Debug.Assert( IsEqualSyntax );
            var movedSpace = SqlTrivia.WhiteSpaceToMiddle( _colName, _asOrEqual, _definition );
            var newAs = _autoAsTNoSpace.SetTrivias( movedSpace.Item2.LeadingTrivias, movedSpace.Item2.TrailingTrivias );
            return new SelectColumn(
                null,
                LeadingTrivias,
                new[] { movedSpace.Item3, newAs, movedSpace.Item1 },
                TrailingTrivias );
        }

        /// <summary>
        /// Returns a column selector that uses the "=" syntax.
        /// </summary>
        /// <returns>A column selector with the "=" syntax.</returns>
        public SelectColumn ToEqualSyntax()
        {
            if( _colName == null || IsEqualSyntax ) return this;
            if( IsHorribleSyntax )
            {
                return new SelectColumn( null, LeadingTrivias, new[] { _colName, _autoAssignTNoSpace, _definition }, TrailingTrivias );
            }
            Debug.Assert( IsAsSyntax );
            var movedSpace = SqlTrivia.WhiteSpaceToMiddle( _definition, _asOrEqual, _colName );
            var newEq = _autoAssignTNoSpace.SetTrivias( movedSpace.Item2.LeadingTrivias, movedSpace.Item2.TrailingTrivias );
            return new SelectColumn(
                null,
                LeadingTrivias,
                new[] { movedSpace.Item3, newEq, movedSpace.Item1 },
                TrailingTrivias );
        }

        /// <summary>
        /// Gets the column name (can be null).
        /// </summary>
        public SqlToken ColumnName => _colName;

        public bool IsEqualSyntax => _asOrEqual is SqlTokenTerminal;

        public bool IsAsSyntax => _asOrEqual is SqlTokenIdentifier;

        public bool IsHorribleSyntax => _asOrEqual == null;

        public SqlToken AsOrEqualT => _asOrEqual;

        public ISqlNode Definition => _definition;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
