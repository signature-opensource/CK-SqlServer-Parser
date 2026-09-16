using CK.Core;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace CK.SqlServer.Parser.Tests;

[TestFixture]
[Category( "SqlTokenizer" )]
public class SqlTokenizerTest
{
    [Test]
    public void column_alias_names()
    {
        SqlTokenType.TableDbType.IsValidColumnAliasName().ShouldBeFalse();
        SqlTokenType.IdentifierStar.IsValidColumnAliasName().ShouldBeFalse();
        SqlTokenType.Create.IsValidColumnAliasName().ShouldBeFalse();
        SqlTokenType.Cursor.IsValidColumnAliasName().ShouldBeFalse();

        SqlTokenType.Throw.IsValidColumnAliasName().ShouldBeTrue();
        SqlTokenType.IdentifierQuoted.IsValidColumnAliasName().ShouldBeTrue();
        SqlTokenType.String.IsValidColumnAliasName().ShouldBeTrue();
        SqlTokenType.UnicodeString.IsValidColumnAliasName().ShouldBeTrue();
        SqlTokenType.IdentifierQuotedBracket.IsValidColumnAliasName().ShouldBeTrue();
        SqlTokenType.IdentifierStandard.IsValidColumnAliasName().ShouldBeTrue();

    }

    [Test]
    public void SimpleTokens()
    {
        var s = "1 = 1 and 0 = 0 and 2 = 2";
        SqlTokenizer t = new SqlTokenizer();
        var e = t.Parse( s ).GetEnumerator();
        (e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "1").ShouldBeTrue();
        (e.MoveNext() && e.Current.TokenType == SqlTokenType.Equal && e.Current.ToString() == "=").ShouldBeTrue();
        (e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "1").ShouldBeTrue();
        (e.MoveNext() && e.Current.TokenType == SqlTokenType.And && e.Current.ToString() == "and").ShouldBeTrue();
        (e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "0").ShouldBeTrue();
        (e.MoveNext() && e.Current.TokenType == SqlTokenType.Equal && e.Current.ToString() == "=").ShouldBeTrue();
        (e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "0").ShouldBeTrue();
        (e.MoveNext() && e.Current.TokenType == SqlTokenType.And && e.Current.ToString() == "and").ShouldBeTrue();
        (e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "2").ShouldBeTrue();
        (e.MoveNext() && e.Current.TokenType == SqlTokenType.Equal && e.Current.ToString() == "=").ShouldBeTrue();
        (e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "2").ShouldBeTrue();
        (e.MoveNext() && e.Current.TokenType == SqlTokenType.EndOfInput).ShouldBeTrue();
        e.MoveNext().ShouldBeFalse();
    }

    [Test]
    public void ToStringHelper()
    {
        SqlTokenizer p = new SqlTokenizer();
        p.ToString().ShouldBe( "<no input>" );

        p.Reset( "a" );
        p.ToString( 1 ).ShouldBe( "a[[HEAD]]" );
        p.Forward();
        p.ToString( 20 ).ShouldBe( "a[[HEAD]]" );

        p.Reset( "aa bb cc dd" );
        p.ToString( 1 ).ShouldBe( "... [[HEAD]]..." );
        p.ToString( 2 ).ShouldBe( "...a [[HEAD]]..." );
        p.ToString( 3 ).ShouldBe( "aa [[HEAD]]..." );
        p.ToString( 4 ).ShouldBe( "aa [[HEAD]]b..." );
        p.Forward();
        p.ToString( 1 ).ShouldBe( "... [[HEAD]]..." );
        p.ToString( 2 ).ShouldBe( "...b [[HEAD]]..." );
        p.ToString( 3 ).ShouldBe( "...bb [[HEAD]]..." );
        p.ToString( 4 ).ShouldBe( "... bb [[HEAD]]..." );
        p.ToString( 5 ).ShouldBe( "...a bb [[HEAD]]..." );
        p.ToString( 6 ).ShouldBe( "aa bb [[HEAD]]..." );
        p.ToString( 7 ).ShouldBe( "aa bb [[HEAD]]c..." );
        p.ToString( 8 ).ShouldBe( "aa bb [[HEAD]]cc..." );
        p.ToString( 9 ).ShouldBe( "aa bb [[HEAD]]cc ..." );
        p.ToString( 10 ).ShouldBe( "aa bb [[HEAD]]cc d..." );
        p.ToString( 11 ).ShouldBe( "aa bb [[HEAD]]cc dd" );
        p.ToString( 1000 ).ShouldBe( "aa bb [[HEAD]]cc dd" );
        p.Forward();
        p.Forward();
        p.ToString( 1 ).ShouldBe( "...d[[HEAD]]" );
        p.ToString( 2 ).ShouldBe( "...dd[[HEAD]]" );
        p.Forward();
        p.ToString( 3 ).ShouldBe( "... dd[[HEAD]]" );
        p.ToString( 4 ).ShouldBe( "...c dd[[HEAD]]" );
        p.ToString( 11 ).ShouldBe( "aa bb cc dd[[HEAD]]" );
    }

    [Test]
    public void EmptyInputAndComments()
    {
        SqlTokenizer p = new SqlTokenizer();

        p.Reset( "" );
        IsEndOfInput( p );

        p.Reset( "\r\n\t " );
        IsEndOfInput( p );
        p.Token.LeadingTrivias.Select( t => t.Text ).ShouldBe( new[] { "\r\n\t " } );
        p.Token.TrailingTrivias.ShouldBeEmpty();

        p.Reset( "\r\n\t  --Comment\r\n \t\r\n /*Other\r\nComment...*/ \r\n" );
        IsEndOfInput( p );
        p.Token.LeadingTrivias.Select( t => t.Text ).ShouldBe( new[] { "\r\n\t  ", "Comment", " \t\r\n ", "Other\r\nComment...", " \r\n" } );
        p.Token.TrailingTrivias.ShouldBeEmpty();
    }

    static void IsEndOfInput( SqlTokenizer p )
    {
        p.Token.ShouldBeAssignableTo<SqlTokenError>().ShouldNotBeNull().IsEndOfInput.ShouldBeTrue();
        p.Token.TokenType.ShouldBe( SqlTokenType.EndOfInput );
        p.Forward().ShouldBeFalse();
        p.Token.ShouldBeAssignableTo<SqlTokenError>().ShouldNotBeNull().IsEndOfInput.ShouldBeTrue();
        p.Token.TokenType.ShouldBe( SqlTokenType.EndOfInput );
    }

    [Test]
    public void SqlTokenType_are_mapped_to_explicit_strings()
    {
        SqlKeyword.ToString( SqlTokenType.IdentifierStandard ).ShouldBe( "¤IdentifierStandard" );
        SqlKeyword.ToString( SqlTokenType.IdentifierQuoted ).ShouldBe( "¤IdentifierQuoted" );
        SqlKeyword.ToString( SqlTokenType.IdentifierQuotedBracket ).ShouldBe( "¤IdentifierQuotedBracket" );
        SqlKeyword.ToString( SqlTokenType.IdentifierVariable ).ShouldBe( "¤IdentifierVariable" );
        SqlKeyword.ToString( SqlTokenType.IdentifierSpecial ).ShouldBe( "¤IdentifierSpecial" );

        Should.Throw<KeyNotFoundException>( () => SqlKeyword.ToString( SqlTokenType.IdentifierReserved ) );
        Should.Throw<KeyNotFoundException>( () => SqlKeyword.ToString( SqlTokenType.IdentifierReservedStatement ) );
        Should.Throw<KeyNotFoundException>( () => SqlKeyword.ToString( SqlTokenType.IdentifierStandardStatement ) );

        SqlKeyword.ToString( SqlTokenType.IdentifierStar ).ShouldBe( "*" );

        SqlKeyword.ToString( SqlTokenType.XmlDbType ).ShouldBe( "xml" );
        SqlKeyword.ToString( SqlTokenType.IntDbType ).ShouldBe( "int" );
        SqlKeyword.ToString( SqlTokenType.VarCharDbType ).ShouldBe( "varchar" );
        SqlKeyword.ToString( SqlTokenType.NVarCharDbType ).ShouldBe( "nvarchar" );
        SqlKeyword.ToString( SqlTokenType.DateTimeDbType ).ShouldBe( "datetime" );
        SqlKeyword.ToString( SqlTokenType.DateTime2DbType ).ShouldBe( "datetime2" );

        SqlKeyword.ToString( SqlTokenType.All ).ShouldBe( "all" );
        SqlKeyword.ToString( SqlTokenType.Authorization ).ShouldBe( "authorization" );
        SqlKeyword.ToString( SqlTokenType.Rows ).ShouldBe( "rows" );
        SqlKeyword.ToString( SqlTokenType.Insert ).ShouldBe( "insert" );
        SqlKeyword.ToString( SqlTokenType.IdentityInsert ).ShouldBe( "identity_insert" );

        SqlKeyword.ToString( SqlTokenType.String ).ShouldBe( "¤String" );
        SqlKeyword.ToString( SqlTokenType.UnicodeString ).ShouldBe( "¤UnicodeString" );
        SqlKeyword.ToString( SqlTokenType.StarComment ).ShouldBe( "¤StarComment" );
        SqlKeyword.ToString( SqlTokenType.LineComment ).ShouldBe( "¤LineComment" );

        SqlKeyword.ToString( SqlTokenType.Integer ).ShouldBe( "¤Integer" );
        SqlKeyword.ToString( SqlTokenType.Float ).ShouldBe( "¤Float" );
        SqlKeyword.ToString( SqlTokenType.Binary ).ShouldBe( "¤Binary" );
        SqlKeyword.ToString( SqlTokenType.Decimal ).ShouldBe( "¤Decimal" );
        SqlKeyword.ToString( SqlTokenType.Money ).ShouldBe( "¤Money" );

        SqlKeyword.ToString( SqlTokenType.GreaterOrEqual ).ShouldBe( ">=" );
        SqlKeyword.ToString( SqlTokenType.Different ).ShouldBe( "!=" );
        SqlKeyword.ToString( SqlTokenType.Assign ).ShouldBe( "=" );
        SqlKeyword.ToString( SqlTokenType.Equal ).ShouldBe( "=" );
    }

    [Test]
    public void Parsing_to_token_types()
    {
        string s = @"
create table [a.b].tC( 
    TheName nvarchar(1254),
    "" w "" numeric(10,7) not null 
);

/* Comment is trivia
(skipped)*/

create procedure [a.b].[sSP] ( @X int , @Y int ) 
as
begin
  declare @g nvarchar ( 42 ) = N'Oups';
  -- a comment (trivia)...
  declare @m money = $78.98;
  exec [a.b] . sOther @p = @X ;
end".Trim();

        string sT = @"
create table ¤IdentifierQuotedBracket.¤IdentifierStandard 
    ( 
        ¤IdentifierStandard nvarchar(¤Integer),
        ¤IdentifierQuoted decimal(¤Integer,¤Integer) not null
    ); 
create procedure ¤IdentifierQuotedBracket.¤IdentifierQuotedBracket( ¤IdentifierVariable int, ¤IdentifierVariable int ) 
as 
begin 
    declare ¤IdentifierVariable nvarchar(¤Integer) = ¤UnicodeString; 
    declare ¤IdentifierVariable money = ¤Money;
    execute ¤IdentifierQuotedBracket.¤IdentifierStandard ¤IdentifierVariable = ¤IdentifierVariable; 
end
¤EndOfInput".Trim();

        var tokenTypes = string.Join( " ", new SqlTokenizer().Parse( s ).Select( t => SqlKeyword.ToString( t.TokenType ) ) );
        Regex.Replace( tokenTypes, @"\s+", string.Empty ).ShouldBe( Regex.Replace( sT, @"\s+", string.Empty ) );
    }

    [TestCase( "A.B.C", 1, "C" )]
    [TestCase( "A.B.C", 2, "B" )]
    [TestCase( "A.B.C", 3, "A" )]
    [TestCase( "A.B.C", 4, null )]
    [TestCase( "[ 1 ]", 1, " 1 " )]
    [TestCase( "[ 1 ] /*k*/ . [ - n°2 - ]", 1, " - n°2 - " )]
    [TestCase( "[ 1 ] /*k*/ . [ - n°2 - ]", 2, " 1 " )]
    [TestCase( "[ 1 ] /*k*/ . [ - n°2 - ]", 3, null )]
    [TestCase( "[ $ ]", 1, " $ " )]
    [TestCase( "[ $ ]", 2, null )]
    public void getting_identifier_part_name( string id, int idxPart, string part )
    {
        ISqlIdentifier t = (ISqlIdentifier)new SqlAnalyser( id ).IsOneExpression( true );
        t.GetPartName( idxPart ).ShouldBe( part );
    }


    [Test]
    public void Float_and_Decimal_with_leading_dot_are_transformed_with_a_leading_Zero_Dot()
    {
        SqlTokenizer p = new SqlTokenizer();
        string s = @"create table [a.b] . tC ( TheName nvarchar ( 1254 ) ) ;
/* Comment is trivia
(not skipped)*/
create procedure [a.b].[sSP]( @X int, @Y int ) 
as
begin
  declare @g nvarchar ( 42 ) = N'Oups'; -- End of line comment...
  exec [a.b].sOther @p = @X, @v = $1235.12;
  declare @x1 decimal = .34;
  declare @x2 float = .45e12;
end".ReplaceLineEndings();
        ISqlTextWriter b = SqlTextWriter.CreateDefault();
        foreach( var t in p.ParseWithoutError( s ) ) t.Write( b );
        string s2 = b.ToString().ReplaceLineEndings();

        // Fix: .34 is changed as 0.34 (decimal), .45e12 becomes 0.45e12 (float).
        s2.ShouldBe( s.Replace( ".34", "0.34" ).Replace( ".45e12", "0.45e12" ) );
    }

    [Test]
    public void removing_quotes_around_identifier_takes_care_of_the_SQL_reserved_keywords()
    {
        SqlTokenizer p = new SqlTokenizer();
        SqlToken t;
        SqlTokenIdentifier tU;
        t = p.ParseWithoutError( "IdentifierStandard" ).ElementAt( 0 );
        t.TokenType.ShouldBe( SqlTokenType.IdentifierStandard );
        t.ShouldBeAssignableTo<SqlTokenIdentifier>().ShouldNotBeNull();
        ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( false ).ShouldBeSameAs( t );
        t.ToString().ShouldBe( "IdentifierStandard" );

        t = p.ParseWithoutError( "[IdentifierQuotedBracket]" ).ElementAt( 0 );
        t.TokenType.ShouldBe( SqlTokenType.IdentifierQuotedBracket );
        t.ShouldBeAssignableTo<SqlTokenIdentifier>().ShouldNotBeNull();
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
        tU.ShouldNotBeSameAs( t );
        t.ToString().ShouldBe( "[IdentifierQuotedBracket]" );
        tU.ToString().ShouldBe( "IdentifierQuotedBracket" );

        t = p.ParseWithoutError( "[Identifier Quoted Bracket]" ).ElementAt( 0 );
        t.TokenType.ShouldBe( SqlTokenType.IdentifierQuotedBracket );
        t.ShouldBeAssignableTo<SqlTokenIdentifier>().ShouldNotBeNull();
        t.ToString().ShouldBe( "[Identifier Quoted Bracket]" );
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
        tU.ShouldBeSameAs( t );

        t = p.ParseWithoutError( "LiKE" ).ElementAt( 0 );
        t.ShouldBeAssignableTo<SqlTokenIdentifier>().ShouldNotBeNull();
        t.ToString().ShouldBe( "LiKE" );
        t.TokenType.ShouldBe( SqlTokenType.Like );

        t = p.ParseWithoutError( "[LiKE]" ).ElementAt( 0 );
        t.TokenType.ShouldBe( SqlTokenType.IdentifierQuotedBracket );
        t.ShouldBeAssignableTo<SqlTokenIdentifier>().ShouldNotBeNull();
        t.ToString().ShouldBe( "[LiKE]" );
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
        tU.ShouldBeSameAs( t );
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( false );
        tU.ShouldNotBeSameAs( t );
        tU.ToString().ShouldBe( "LiKE" );
        tU.TokenType.ShouldBe( SqlTokenType.Like );

        t = p.ParseWithoutError( "IN" ).ElementAt( 0 );
        t.TokenType.ShouldBe( SqlTokenType.In );
        t.ShouldBeAssignableTo<SqlTokenIdentifier>().ShouldNotBeNull();
        t.ToString().ShouldBe( "IN" );

        t = p.ParseWithoutError( "[IN]" ).ElementAt( 0 );
        t.TokenType.ShouldBe( SqlTokenType.IdentifierQuotedBracket );
        t.ShouldBeAssignableTo<SqlTokenIdentifier>().ShouldNotBeNull();
        t.ToString().ShouldBe( "[IN]" );
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
        tU.ShouldBeSameAs( t );
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( false );
        tU.ShouldNotBeSameAs( t );
        tU.ToString().ShouldBe( "IN" );
        tU.TokenType.ShouldBe( SqlTokenType.In );

        t = p.ParseWithoutError( "int" ).ElementAt( 0 );
        t.TokenType.ShouldBe( SqlTokenType.IntDbType );
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
        tU.ShouldBeSameAs( t );

        t = p.ParseWithoutError( "[int]" ).ElementAt( 0 );
        t.TokenType.ShouldBe( SqlTokenType.IdentifierQuotedBracket );
        t.ShouldBeAssignableTo<SqlTokenIdentifier>().ShouldNotBeNull();
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
        tU.ShouldNotBeSameAs( t );
        tU.ToString().ShouldBe( "int" );
        tU.TokenType.ShouldBe( SqlTokenType.IntDbType );
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( false );
        tU.ShouldNotBeSameAs( t );
        tU.ToString().ShouldBe( "int" );
        tU.TokenType.ShouldBe( SqlTokenType.IntDbType );

        t = p.ParseWithoutError( @"""smalliNt""" ).ElementAt( 0 );
        t.TokenType.ShouldBe( SqlTokenType.IdentifierQuoted );
        t.ShouldBeAssignableTo<SqlTokenIdentifier>().ShouldNotBeNull();
        t.ToString().ShouldBe( @"""smalliNt""" );
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
        tU.ShouldNotBeSameAs( t );
        tU.ToString().ShouldBe( "smalliNt" );
        tU.TokenType.ShouldBe( SqlTokenType.SmallIntDbType );
        tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( false );
        tU.ShouldNotBeSameAs( t );
        tU.ToString().ShouldBe( "smalliNt" );
        tU.TokenType.ShouldBe( SqlTokenType.SmallIntDbType );

    }

    [Test]
    public void parsing_numbers_and_money()
    {
        SqlTokenizer p = new SqlTokenizer();

        AssertRewrite( p, "0", "0" );
        AssertRewrite( p, "0.23", "0.23" );
        AssertRewrite( p, ".23", "0.23" );
        AssertRewrite( p, "0000.23", "0.23" );

        AssertRewrite( p, "$", "$0" );
        //AssertRewrite( p, "$1", "$1" );
        AssertRewrite( p, "$1.", "$1.0" );
        AssertRewrite( p, "£.233", "£0.233" );
        AssertRewrite( p, "£    .23", "£0.23" );
        AssertRewrite( p, "$   0000.23", "$0.23" );

        AssertRewrite( p, ".45E+12", "0.45e12" );
        AssertRewrite( p, "00012.147e-4", "12.147e-4" );
    }

    static void AssertRewrite( SqlTokenizer p, string toParse, string rewritten )
    {
        ISqlTextWriter b = SqlTextWriter.CreateDefault();
        foreach( var t in p.ParseWithoutError( toParse ) ) t.Write( b );
        string r = b.ToString();
        r.ShouldBe( rewritten );
    }

    [Test]
    public void Comments_are_Trivias()
    {
        string s = @"'' -- CancelDate";
        SqlTokenizer p = new SqlTokenizer();
        var tokens = p.Parse( s ).ToArray();
        tokens.Length.ShouldBe( 2 );
        tokens[0].TokenType.ShouldBe( SqlTokenType.String );
        tokens[0].TrailingTrivias.Count.ShouldBe( 2 );
        tokens[0].TrailingTrivias[0].TokenType.ShouldBe( SqlTokenType.None );
        tokens[0].TrailingTrivias[0].Text.ShouldBe( " " );
        tokens[0].TrailingTrivias[1].TokenType.ShouldBe( SqlTokenType.LineComment );
        tokens[0].TrailingTrivias[1].Text.ShouldBe( " CancelDate" );
        tokens[1].TokenType.ShouldBe( SqlTokenType.EndOfInput );
    }

    [Test]
    public void LineComment_eats_its_prefix_and_LineTermination()
    {
        string s = @"'' -- CancelDate
TOKEN";
        SqlTokenizer p = new SqlTokenizer();
        var tokens = p.Parse( s ).ToArray();
        tokens.Length.ShouldBe( 3 );
        tokens[0].TokenType.ShouldBe( SqlTokenType.String );
        tokens[0].TrailingTrivias.Count.ShouldBe( 2 );
        tokens[0].TrailingTrivias[0].TokenType.ShouldBe( SqlTokenType.None );
        tokens[0].TrailingTrivias[0].Text.ShouldBe( " " );
        tokens[0].TrailingTrivias[1].TokenType.ShouldBe( SqlTokenType.LineComment );
        tokens[0].TrailingTrivias[1].Text.ShouldBe( " CancelDate", "No line endings in Text." );
        tokens[1].TokenType.ShouldBe( SqlTokenType.IdentifierStandard );
        tokens[1].LeadingTrivias.Count.ShouldBe( 0 );
        tokens[1].TrailingTrivias.Count.ShouldBe( 0 );
        tokens[2].TokenType.ShouldBe( SqlTokenType.EndOfInput );
    }

    [Test]
    public void empty_LineComments_are_kept()
    {
        string s = @"
insert -- Comment1
--
-- Comment2
--
-- Comment3 X   
identifer2";
        SqlTokenizer p = new SqlTokenizer();
        var tokens = p.Parse( s ).ToArray();

        tokens.Length.ShouldBe( 3 );

        tokens[0].LeadingTrivias.Count.ShouldBe( 1 );
        tokens[0].LeadingTrivias[0].Text.ShouldBe( Environment.NewLine );
        tokens[0].IsToken( SqlTokenType.Insert ).ShouldBeTrue();
        tokens[0].TrailingTrivias.Count.ShouldBe( 2 );
        tokens[0].TrailingTrivias[0].Text.ShouldBe( " " );
        tokens[0].TrailingTrivias[1].Text.ShouldBe( " Comment1" );

        tokens[1].LeadingTrivias.Count.ShouldBe( 4 );
        tokens[1].LeadingTrivias[0].Text.ShouldBe( string.Empty );
        tokens[1].LeadingTrivias[1].Text.ShouldBe( " Comment2" );
        tokens[1].LeadingTrivias[2].Text.ShouldBe( string.Empty );
        tokens[1].LeadingTrivias[3].Text.ShouldBe( " Comment3 X   " );

        tokens[1].IsToken( SqlTokenType.IdentifierStandard ).ShouldBeTrue();
        tokens[1].TrailingTrivias.Count.ShouldBe( 0 );

        tokens[2].TokenType.ShouldBe( SqlTokenType.EndOfInput );

    }

    [TestCase( "[A[x]]]", "A[x]" )]
    [TestCase( @"""A""""x""""""", @"A""x""" )]
    [TestCase( @"'A''x'''", @"A'x'" )]
    public void strings_and_quoted_identifiers_are_funny_beasts( string text, string result )
    {
        text = text.Replace( "x", Environment.NewLine + "x" + Environment.NewLine );
        result = result.Replace( "x", Environment.NewLine + "x" + Environment.NewLine );
        SqlTokenizer p = new SqlTokenizer();
        var t = p.Parse( text ).Single( x => x.TokenType != SqlTokenType.EndOfInput );
        (t is SqlTokenIdentifier or SqlTokenLiteralString).ShouldBeTrue();
        if( t is SqlTokenIdentifier id )
        {
            id.Name.ShouldBe( result );
        }
        else
        {
            var str = (SqlTokenLiteralString)t;
            str.Value.ShouldBe( result );
        }
    }

    [TestCase( "0x", "[]" )]
    [TestCase( @"0x1", "[1]" )]
    [TestCase( @"0xF", "[15]" )]
    [TestCase( @"0xFF", "[255]" )]
    [TestCase( @"0xFF1", "[15,241]" )]
    [TestCase( @"0xFFF", "[15,255]" )]
    [TestCase( @"0xFFFF", "[255,255]" )]
    [TestCase( @"0xFFFFF", "[15,255,255]" )]
    public void literal_binary_values_are_handled_as_byte_array( string text, string result )
    {
        SqlTokenizer p = new SqlTokenizer();
        var t = p.Parse( text ).Single( x => x.TokenType != SqlTokenType.EndOfInput );
        t.ShouldBeAssignableTo<SqlTokenLiteralBinary>().ShouldNotBeNull();
        SqlBasicValue v = new SqlBasicValue( null, (SqlTokenLiteralBinary)t );
        byte[] val = (byte[])v.NullOrLitteralDotNetValue;
        var actual = "[" + val.Select( b => b.ToString() ).Concatenate( "," ) + "]";
        actual.ShouldBe( result );
    }

}
