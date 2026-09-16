using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace CK.SqlServer.Parser.Tests.Parsing;

[TestFixture]
public class SqlTokenReaderTests
{
    static SqlTokenReader CreateReader( string s )
    {
        SqlTokenizer t = new SqlTokenizer();
        var r = new SqlTokenReader( t );
        r.Reset( s );
        r.MoveNext();
        return r;
    }

    [Test]
    public void TokenTypes()
    {
        {
            SqlTokenReader r = CreateReader( "as" );
            r.Current.TokenType.ShouldBe( SqlTokenType.As );
            SqlTokenIdentifier asToken;
            r.IsToken( out asToken, SqlTokenType.As, true ).ShouldBeTrue();
        }
        {
            SqlTokenReader r = CreateReader( "[as]" );
            r.Current.TokenType.ShouldNotBe( SqlTokenType.As );
            SqlTokenIdentifier asToken;
            r.IsToken( out asToken, SqlTokenType.As, expected: true ).ShouldBeFalse();
            r.IsError.ShouldBeTrue();
        }
    }
}
