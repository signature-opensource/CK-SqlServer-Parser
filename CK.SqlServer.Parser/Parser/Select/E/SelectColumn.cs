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
    public sealed class SelectColumn : ASqlNodeArrayBased
    {
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

        SelectColumn( SelectColumn o, ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            if( items == null )
            {
                _definition = o._definition;
                _asOrEqual = o._asOrEqual;
                _colName = o._colName;
            }
            else
            {
                if( Children.Length == 1 ) _definition = Children[0];
                else
                {
                    if( Children.Length == 2 )
                    {
                        _definition = Children[0];
                        _colName = (SqlToken)Children[1];
                        SNode.CheckToken( ColumnName, nameof( ColumnName ), SqlTokenTypeExtension.IsValidColumnAliasName );
                    }
                    else
                    {
                        if( Children.Length > 3 )
                        {
                            throw new ArgumentException( "At most 3 parts must be provided." );
                        }
                        _asOrEqual = (SqlToken)Children[1];
                        if( _asOrEqual is SqlTokenTerminal )
                        {
                            _colName = (SqlToken)Children[0];
                            SNode.CheckToken( AsOrEqualT, nameof( AsOrEqualT ), SqlTokenType.Assign );
                            _definition = Children[2];
                            SNode.CheckToken( ColumnName, nameof( ColumnName ), SqlTokenTypeExtension.IsValidColumnAliasNameOrVariable );
                        }
                        else
                        {
                            _colName = (SqlToken)Children[2];
                            SNode.CheckToken( AsOrEqualT, nameof( AsOrEqualT ), SqlTokenType.As );
                            _definition = Children[0];
                            SNode.CheckToken( ColumnName, nameof( ColumnName ), SqlTokenTypeExtension.IsValidColumnAliasName );
                        }
                    }
                }
                SNode.CheckNotNull( Definition, nameof( Definition ) );
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectColumn( this, leading, EnsureArray( children ), trailing );
        }

        SqlTokenIdentifier GetRepairedAsToken()
        {
            SqlTokenIdentifier asT;
            var leftTrivia = _definition.FullTrailingTrivias.Any();
            var rightTrivia = _colName.FullLeadingTrivias.Any();
            if( !leftTrivia )
            {
                if( !rightTrivia ) asT = _autoAsT;
                else asT = _autoAsTNoRight;
            }
            else
            {
                if( !rightTrivia )
                    asT = _autoAsTNoLeft;
                else asT = _autoAsTNoSpace;
            }
            return asT;
        }

        public SelectColumn ToAsSyntax()
        {
            if( _colName == null || IsAsSyntax ) return this;
            if( IsHorribleSyntax )
            {
                return new SelectColumn( null, LeadingTrivias, new[] { _definition, GetRepairedAsToken(), _colName }, TrailingTrivias );
            }
            Debug.Assert( IsEqualSyntax );
            var newName = _colName.SetTrivias( _definition.FullLeadingTrivias, _definition.FullTrailingTrivias );
            var newDef = _definition.SetTrivias( _colName.FullLeadingTrivias, _colName.FullTrailingTrivias );
            var newAs = _autoAsTNoSpace.SetTrivias( _asOrEqual.LeadingTrivias, _asOrEqual.TrailingTrivias );
            return new SelectColumn(
                null,
                LeadingTrivias,
                new[] { newDef, newAs, newName },
                TrailingTrivias );
        }

        public SelectColumn ToEqualSyntax()
        {
            if( _colName == null || IsEqualSyntax ) return this;
            if( IsHorribleSyntax ) return ToAsSyntax().ToEqualSyntax();
            Debug.Assert( IsAsSyntax );
            var newName = _colName.SetTrivias( _definition.FullLeadingTrivias, _definition.FullTrailingTrivias );
            var newDef = _definition.SetTrivias( _colName.FullLeadingTrivias, _colName.FullTrailingTrivias );
            var newEq = _autoAssignTNoSpace.SetTrivias( _asOrEqual.LeadingTrivias, _asOrEqual.TrailingTrivias );
            return new SelectColumn(
                null,
                LeadingTrivias,
                new[] { newName, newEq, newDef },
                TrailingTrivias );
        }

        public SqlToken ColumnName => _colName;

        public bool IsEqualSyntax => _asOrEqual is SqlTokenTerminal;

        public bool IsAsSyntax => _asOrEqual is SqlTokenIdentifier;

        public bool IsHorribleSyntax => _asOrEqual == null;

        public SqlToken AsOrEqualT => _asOrEqual;

        public ISqlNode Definition => _definition;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
