using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


namespace CK.SqlServer.Parser;

/// <summary>
/// List of possibly empty <see cref="ISqlStatement">statements</see>. 
/// </summary>
public sealed class SqlStatementList : ASqlNodeList<ISqlStatement>, ISqlServerScript
{
    public SqlStatementList( IEnumerable<ISqlStatement> statements )
        : base( 0, statements )
    {
    }

    SqlStatementList( SqlStatementList o, ImmutableList<SqlTrivia> leading, IEnumerable<ISqlNode> statements, ImmutableList<SqlTrivia> trailing )
        : base( o, 0, leading, statements, trailing )
    {
    }

    protected override SqlNode DoClone( ImmutableList<SqlTrivia> leading, IList<ISqlNode> content, ImmutableList<SqlTrivia> trailing )
    {
        return new SqlStatementList( this, leading, content, trailing );
    }

    IEnumerable<ISqlServerComment> ISqlServerParsedText.HeaderComments
    {
        get { return FullLeadingTrivias.Where( t => t.TokenType != SqlTokenType.None ).Cast<ISqlServerComment>(); }
    }

    void ISqlServerParsedText.Write( StringBuilder b ) => Write( SqlTextWriter.CreateDefault( b ) );

    [DebuggerStepThrough]
    internal protected override ISqlNode Accept( SqlNodeVisitor visitor ) => visitor.Visit( this );
}
