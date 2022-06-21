using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Captures a select column definition: it is either 'definition as name', 'name = definition' or the definition alone.
    /// The horrible syntax 'definition name' is also supported.
    /// </summary>
    public sealed class SelectColumn : SqlNonTokenAutoWidth
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


        /// <summary>
        /// Initializes a new column with '=': <see cref="ColumnName"/> = <see cref="Definition"/>.
        /// This constructor allows <paramref name="colName"/> to be a variable name.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="assignT">The assign token.</param>
        /// <param name="colName">The column name variable name or string, quoted identifier or standard identifier.</param>
        public SelectColumn( SqlToken colName, SqlTokenTerminal assignT, ISqlNode definition )
            : this( null, null, new[] { colName, assignT, definition }, null )
        {
        }

        /// <summary>
        /// Initializes a new column with 'as': <see cref="Definition"/> as <see cref="ColumnName"/>.
        /// This constructor implies that <paramref name="colName"/> can not be a variable name.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="asT">The as token.</param>
        /// <param name="colName">The column name (string, quoted identifier or standard identifier), but not a variable name.</param>
        public SelectColumn( ISqlNode definition, SqlTokenIdentifier asT, SqlToken colName )
            : this( null, null, new[] { definition, asT, colName }, null )
        {
        }

        /// <summary>
        /// Initializes a new column with no 'as' nor '=': <see cref="Definition"/> <see cref="ColumnName"/>.
        /// This constructor implies that <paramref name="colName"/> can not be a variable name.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="colName">The column name (string, quoted identifier or standard identifier), but not a variable name.</param>
        public SelectColumn( ISqlNode definition, SqlToken colName )
            : this( null, null, new[] { definition, colName }, null )
        {
        }

        /// <summary>
        /// Initializes a new column with no <see cref="ColumnName"/>).
        /// </summary>
        /// <param name="definition">The definition.</param>
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

        /// <summary>
        /// Gets a <see cref="SqlTokenIdentifier"/> for the <see cref="ColumnName"/>, whatever it is.
        /// It is the ColumnName if it is an identifier, a [quoted dentifier] if ColumnName is a 'string' or 
        /// a N'string' and a standard identifier with the fallback name if ColumnName is null.
        /// </summary>
        /// <param name="fallbackIdentifierName">Identifier name to use if <see cref="ColumnName"/> is null.</param>
        /// <returns>An identifier.</returns>
        public SqlTokenIdentifier GetAutoColumnNameIdentifier( string fallbackIdentifierName )
        {
            SqlTokenIdentifier idName = ColumnName as SqlTokenIdentifier;
            if( idName == null )
            {
                if( ColumnName == null ) idName = new SqlTokenIdentifier( SqlTokenType.IdentifierStandard, fallbackIdentifierName );
                else idName = new SqlTokenIdentifier( SqlTokenType.IdentifierQuotedBracket, ((ISqlHasStringValue)ColumnName).Value );
            }
            return idName;
        }

        public bool IsEqualSyntax => _asOrEqual is SqlTokenTerminal;

        public bool IsAsSyntax => _asOrEqual is SqlTokenIdentifier;

        public bool IsHorribleSyntax => _asOrEqual == null;

        public SqlToken AsOrEqualT => _asOrEqual;

        public ISqlNode Definition => _definition;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );

    }


}
