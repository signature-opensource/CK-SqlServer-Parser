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
    /// The horrible syntax 'definition name' is silently transformed into 'defintion as name'.
    /// </summary>
    public sealed class SelectColumn : ASqlNodeArrayBased
    {
        readonly ISqlIdentifier _colName;
        readonly SqlToken _asOrEqual;
        readonly ISqlNode _definition;

        static readonly SqlTokenIdentifier _autoAsT = new SqlTokenIdentifier( SqlTokenType.As, "as", SqlTrivia.OneSpace, SqlTrivia.OneSpace );
        static readonly SqlTokenIdentifier _autoAsTNoLeft = new SqlTokenIdentifier( SqlTokenType.As, "as", null, SqlTrivia.OneSpace );
        static readonly SqlTokenIdentifier _autoAsTNoRight = new SqlTokenIdentifier( SqlTokenType.As, "as", SqlTrivia.OneSpace, null );
        static readonly SqlTokenIdentifier _autoAsTNoSpace = new SqlTokenIdentifier( SqlTokenType.As, "as", null, null );

        public SelectColumn( ISqlIdentifier colName, SqlTokenTerminal assignT, ISqlNode definition )
            : this( null, null, Build( colName, assignT, definition ), null )
        {
        }

        public SelectColumn( ISqlNode definition, SqlTokenIdentifier asT, ISqlIdentifier colName )
            : this( null, null, Build( definition, asT, colName ), null )
        {
        }

        public SelectColumn( ISqlNode definition, ISqlIdentifier colName = null )
            : this( null, null, Build( definition, colName ), null )
        {
        }

        static ISqlNode[] Build( ISqlIdentifier colName, SqlTokenTerminal assignT, ISqlNode definition )
        {
            if( colName == null ) throw new ArgumentNullException( "colName" );
            if( assignT == null ) throw new ArgumentNullException( "assignT" );
            if( assignT.TokenType != SqlTokenType.Assign ) throw new ArgumentException( "Assign token expected.", "assignT" );
            if( definition == null ) throw new ArgumentNullException( "definition" );
            return CreateArray( (SqlNode)colName, assignT, definition );
        }

        static ISqlNode[] Build( ISqlNode definition, SqlTokenIdentifier asT, ISqlIdentifier colName )
        {
            if( definition == null ) throw new ArgumentNullException( "definition" );
            if( colName == null ) throw new ArgumentNullException( "colName" );
            if( asT == null )
            {
                var leftTrivia = definition.FullTrailingTrivias.Any();
                var rightTrivia = colName.FullLeadingTrivias.Any();
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
            }
            else if( asT.TokenType != SqlTokenType.As ) throw new ArgumentException( "As token expected.", "asT" );
            return CreateArray( definition, asT, (SqlNode)colName );
        }

        static ISqlNode[] Build( ISqlNode definition, ISqlIdentifier colName )
        {
            if( definition == null ) throw new ArgumentNullException( "definition" );
            if( colName == null ) return CreateArray( definition );
            return Build( definition, null, colName );
        }

        SelectColumn( SelectColumn o, ImmutableList<SqlTrivia> leading, ISqlNode[] items, ImmutableList<SqlTrivia> trailing )
            : base( leading, items, trailing )
        {
            if( Children.Length == 1 ) _definition = Children[0];
            else
            {
                _asOrEqual = (SqlToken)Children[1];
                if( _asOrEqual is SqlTokenTerminal )
                {
                    _colName = (ISqlIdentifier)Children[0];
                    _definition = Children[2];
                }
                else
                {
                    _colName = (ISqlIdentifier)Children[2];
                    _definition = Children[0];
                }
            }
        }

        protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IReadOnlyList<ISqlNode> children, ImmutableList<SqlTrivia> trailing )
        {
            return new SelectColumn( this, leading, EnsureArray( children ), trailing );
        }

        public ISqlIdentifier ColumnName => _colName;

        public bool IsEqualSyntax => _asOrEqual is SqlTokenTerminal;

        public bool IsAsSyntax => _asOrEqual is SqlTokenIdentifier;

        public SqlToken AsOrEqualT => _asOrEqual;
        
        public ISqlNode Definition => _definition;

        [DebuggerStepThrough]
        internal protected override ISqlNode Accept( SqlItemVisitor visitor ) => visitor.Visit( this );

    }


}
