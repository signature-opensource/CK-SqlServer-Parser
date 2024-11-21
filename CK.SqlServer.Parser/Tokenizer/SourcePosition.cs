using System.Globalization;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

public struct SourcePosition
{
    public readonly int Line;
    public readonly int Column;

    public SourcePosition( int line, int column )
    {
        Line = line;
        Column = column;
    }

    public override string ToString()
    {
        return '@' + Line.ToString( CultureInfo.InvariantCulture ) + ',' + Column.ToString( CultureInfo.InvariantCulture );
    }
}
