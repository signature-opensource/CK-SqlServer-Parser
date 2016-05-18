using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.SqlServer;
using System.Text.RegularExpressions;
using CK.SqlServer.UtilTests;

namespace CK.SqlServer.Parser.Tests
{
    [TestFixture]
    [Category( "SqlTokenizer" )]
    public class SqlTokenizerTest
    {
        [Test]
        public void column_alias_names()
        {
            Assert.That( SqlTokenType.TableDbType.IsValidColumnAliasName(), Is.False );
            Assert.That( SqlTokenType.IdentifierStar.IsValidColumnAliasName(), Is.False );
            Assert.That( SqlTokenType.Create.IsValidColumnAliasName(), Is.False );
            Assert.That( SqlTokenType.Cursor.IsValidColumnAliasName(), Is.False );

            Assert.That( SqlTokenType.Throw.IsValidColumnAliasName(), Is.True );
            Assert.That( SqlTokenType.IdentifierQuoted.IsValidColumnAliasName(), Is.True );
            Assert.That( SqlTokenType.String.IsValidColumnAliasName(), Is.True );
            Assert.That( SqlTokenType.UnicodeString.IsValidColumnAliasName(), Is.True );
            Assert.That( SqlTokenType.IdentifierQuotedBracket.IsValidColumnAliasName(), Is.True );
            Assert.That( SqlTokenType.IdentifierStandard.IsValidColumnAliasName(), Is.True );

        }

        [Test]
        public void SimpleTokens()
        {
            var s = "1 = 1 and 0 = 0 and 2 = 2";
            SqlTokenizer t = new SqlTokenizer();
            var e = t.Parse( s ).GetEnumerator();
            Assert.That( e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "1" );
            Assert.That( e.MoveNext() && e.Current.TokenType == SqlTokenType.Equal && e.Current.ToString() == "=" );
            Assert.That( e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "1" );
            Assert.That( e.MoveNext() && e.Current.TokenType == SqlTokenType.And && e.Current.ToString() == "and" );
            Assert.That( e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "0" );
            Assert.That( e.MoveNext() && e.Current.TokenType == SqlTokenType.Equal && e.Current.ToString() == "=" );
            Assert.That( e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "0" );
            Assert.That( e.MoveNext() && e.Current.TokenType == SqlTokenType.And && e.Current.ToString() == "and" );
            Assert.That( e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "2" );
            Assert.That( e.MoveNext() && e.Current.TokenType == SqlTokenType.Equal && e.Current.ToString() == "=" );
            Assert.That( e.MoveNext() && (e.Current.TokenType & SqlTokenType.IsNumber) != 0 && e.Current.ToString() == "2" );
            Assert.That( e.MoveNext() && e.Current.TokenType == SqlTokenType.EndOfInput );
            Assert.That( !e.MoveNext() );
        }

        [Test]
        public void ToStringHelper()
        {
            SqlTokenizer p = new SqlTokenizer();
            Assert.That( p.ToString(), Is.EqualTo( "<no input>" ) );

            p.Reset( "a" );
            Assert.That( p.ToString( 1 ), Is.EqualTo( "a[[HEAD]]" ) );
            p.Forward();
            Assert.That( p.ToString( 20 ), Is.EqualTo( "a[[HEAD]]" ) );

            p.Reset( "aa bb cc dd" );
            Assert.That( p.ToString( 1 ), Is.EqualTo( "... [[HEAD]]..." ) );
            Assert.That( p.ToString( 2 ), Is.EqualTo( "...a [[HEAD]]..." ) );
            Assert.That( p.ToString( 3 ), Is.EqualTo( "aa [[HEAD]]..." ) );
            Assert.That( p.ToString( 4 ), Is.EqualTo( "aa [[HEAD]]b..." ) );
            p.Forward();
            Assert.That( p.ToString( 1 ), Is.EqualTo( "... [[HEAD]]..." ) );
            Assert.That( p.ToString( 2 ), Is.EqualTo( "...b [[HEAD]]..." ) );
            Assert.That( p.ToString( 3 ), Is.EqualTo( "...bb [[HEAD]]..." ) );
            Assert.That( p.ToString( 4 ), Is.EqualTo( "... bb [[HEAD]]..." ) );
            Assert.That( p.ToString( 5 ), Is.EqualTo( "...a bb [[HEAD]]..." ) );
            Assert.That( p.ToString( 6 ), Is.EqualTo( "aa bb [[HEAD]]..." ) );
            Assert.That( p.ToString( 7 ), Is.EqualTo( "aa bb [[HEAD]]c..." ) );
            Assert.That( p.ToString( 8 ), Is.EqualTo( "aa bb [[HEAD]]cc..." ) );
            Assert.That( p.ToString( 9 ), Is.EqualTo( "aa bb [[HEAD]]cc ..." ) );
            Assert.That( p.ToString( 10 ), Is.EqualTo( "aa bb [[HEAD]]cc d..." ) );
            Assert.That( p.ToString( 11 ), Is.EqualTo( "aa bb [[HEAD]]cc dd" ) );
            Assert.That( p.ToString( 1000 ), Is.EqualTo( "aa bb [[HEAD]]cc dd" ) );
            p.Forward();
            p.Forward();
            Assert.That( p.ToString( 1 ), Is.EqualTo( "...d[[HEAD]]" ) );
            Assert.That( p.ToString( 2 ), Is.EqualTo( "...dd[[HEAD]]" ) );
            p.Forward();
            Assert.That( p.ToString( 3 ), Is.EqualTo( "... dd[[HEAD]]" ) );
            Assert.That( p.ToString( 4 ), Is.EqualTo( "...c dd[[HEAD]]" ) );
            Assert.That( p.ToString( 11 ), Is.EqualTo( "aa bb cc dd[[HEAD]]" ) );
        }

        [Test]
        public void EmptyInputAndComments()
        {
            SqlTokenizer p = new SqlTokenizer();

            p.Reset( "" );
            IsEndOfInput( p );

            p.Reset( "\r\n\t " );
            IsEndOfInput( p );
            CollectionAssert.AreEquivalent( p.Token.LeadingTrivias.Select( t => t.Text ), new[] { "\r\n\t " } );
            CollectionAssert.IsEmpty( p.Token.TrailingTrivias );

            p.Reset( "\r\n\t  --Comment\r\n \t\r\n /*Other\r\nComment...*/ \r\n" );
            IsEndOfInput( p );
            CollectionAssert.AreEquivalent( p.Token.LeadingTrivias.Select( t => t.Text ), new[] { "\r\n\t  ", "Comment", " \t\r\n ", "Other\r\nComment...", " \r\n" } );
            CollectionAssert.IsEmpty( p.Token.TrailingTrivias );
        }

        private static void IsEndOfInput( SqlTokenizer p )
        {
            Assert.That( p.Token is SqlTokenError );
            Assert.That( p.Token.TokenType, Is.EqualTo( SqlTokenType.EndOfInput ) );
            Assert.That( ((SqlTokenError)p.Token).IsEndOfInput );
            Assert.That( !p.Forward() );
            Assert.That( p.Token is SqlTokenError );
            Assert.That( p.Token.TokenType, Is.EqualTo( SqlTokenType.EndOfInput ) );
            Assert.That( ((SqlTokenError)p.Token).IsEndOfInput );
        }

        [Test]
        public void SqlTokenType_are_mapped_to_explicit_strings()
        {
            Assert.That( SqlKeyword.ToString( SqlTokenType.IdentifierStandard ), Is.EqualTo( "¤IdentifierStandard" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.IdentifierQuoted ), Is.EqualTo( "¤IdentifierQuoted" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.IdentifierQuotedBracket ), Is.EqualTo( "¤IdentifierQuotedBracket" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.IdentifierVariable ), Is.EqualTo( "¤IdentifierVariable" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.IdentifierSpecial ), Is.EqualTo( "¤IdentifierSpecial" ) );

            Assert.Throws<KeyNotFoundException>( () => SqlKeyword.ToString( SqlTokenType.IdentifierReserved ) );
            Assert.Throws<KeyNotFoundException>( () => SqlKeyword.ToString( SqlTokenType.IdentifierReservedStatement ) );
            Assert.Throws<KeyNotFoundException>( () => SqlKeyword.ToString( SqlTokenType.IdentifierStandardStatement ) );

            Assert.That( SqlKeyword.ToString( SqlTokenType.IdentifierStar ), Is.EqualTo( "*" ) );

            Assert.That( SqlKeyword.ToString( SqlTokenType.XmlDbType ), Is.EqualTo( "xml" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.IntDbType ), Is.EqualTo( "int" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.VarCharDbType ), Is.EqualTo( "varchar" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.NVarCharDbType ), Is.EqualTo( "nvarchar" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.DateTimeDbType ), Is.EqualTo( "datetime" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.DateTime2DbType ), Is.EqualTo( "datetime2" ) );

            Assert.That( SqlKeyword.ToString( SqlTokenType.String ), Is.EqualTo( "¤String" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.UnicodeString ), Is.EqualTo( "¤UnicodeString" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.StarComment ), Is.EqualTo( "¤StarComment" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.LineComment ), Is.EqualTo( "¤LineComment" ) );

            Assert.That( SqlKeyword.ToString( SqlTokenType.Integer ), Is.EqualTo( "¤Integer" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.Float ), Is.EqualTo( "¤Float" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.Binary ), Is.EqualTo( "¤Binary" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.Decimal ), Is.EqualTo( "¤Decimal" ) );
            Assert.That( SqlKeyword.ToString( SqlTokenType.Money ), Is.EqualTo( "¤Money" ) );
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
            Assert.That( Regex.Replace( tokenTypes, @"\s+", string.Empty ), Is.EqualTo( Regex.Replace( sT, @"\s+", string.Empty ) ) );
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
            Assert.That( t.GetPartName( idxPart ), Is.EqualTo( part ) );
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
end".NormalizeEOL();
            ISqlTextWriter b = SqlTextWriter.CreateDefault();
            foreach( var t in p.ParseWithoutError( s ) ) t.Write( b );
            string s2 = b.ToString().NormalizeEOL();

            // Fix: .34 is changed as 0.34 (decimal), .45e12 becomes 0.45e12 (float).
            Assert.That( s2, Is.EqualTo( s.Replace( ".34", "0.34" ).Replace( ".45e12", "0.45e12" ) ) );
        }

        [Test]
        public void removing_quotes_around_identifier_takes_care_of_the_SQL_reserved_keywords()
        {
            SqlTokenizer p = new SqlTokenizer();
            SqlToken t;
            SqlTokenIdentifier tU;
            t = p.ParseWithoutError( "IdentifierStandard" ).ElementAt( 0 );
            Assert.That( t.TokenType == SqlTokenType.IdentifierStandard );
            Assert.That( t is SqlTokenIdentifier );
            Assert.That( ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( false ), Is.SameAs( t ) );
            Assert.That( t.ToString(), Is.EqualTo( "IdentifierStandard" ) );

            t = p.ParseWithoutError( "[IdentifierQuotedBracket]" ).ElementAt( 0 );
            Assert.That( t.TokenType == SqlTokenType.IdentifierQuotedBracket );
            Assert.That( t is SqlTokenIdentifier );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
            Assert.That( tU, Is.Not.SameAs( t ) );
            Assert.That( t.ToString(), Is.EqualTo( "[IdentifierQuotedBracket]" ) );
            Assert.That( tU.ToString(), Is.EqualTo( "IdentifierQuotedBracket" ) );

            t = p.ParseWithoutError( "[Identifier Quoted Bracket]" ).ElementAt( 0 );
            Assert.That( t.TokenType == SqlTokenType.IdentifierQuotedBracket );
            Assert.That( t is SqlTokenIdentifier );
            Assert.That( t.ToString(), Is.EqualTo( "[Identifier Quoted Bracket]" ) );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
            Assert.That( tU, Is.SameAs( t ) );

            t = p.ParseWithoutError( "LiKE" ).ElementAt( 0 );
            Assert.That( t is SqlTokenIdentifier );
            Assert.That( t.ToString(), Is.EqualTo( "LiKE" ) );
            Assert.That( t.TokenType == SqlTokenType.Like );

            t = p.ParseWithoutError( "[LiKE]" ).ElementAt( 0 );
            Assert.That( t.TokenType == SqlTokenType.IdentifierQuotedBracket );
            Assert.That( t is SqlTokenIdentifier );
            Assert.That( t.ToString(), Is.EqualTo( "[LiKE]" ) );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
            Assert.That( tU, Is.SameAs( t ) );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( false );
            Assert.That( tU, Is.Not.SameAs( t ) );
            Assert.That( tU.ToString(), Is.EqualTo( "LiKE" ) );
            Assert.That( tU.TokenType, Is.EqualTo( SqlTokenType.Like ) );

            t = p.ParseWithoutError( "IN" ).ElementAt( 0 );
            Assert.That( t.TokenType == SqlTokenType.In );
            Assert.That( t is SqlTokenIdentifier );
            Assert.That( t.ToString(), Is.EqualTo( "IN" ) );

            t = p.ParseWithoutError( "[IN]" ).ElementAt( 0 );
            Assert.That( t.TokenType == SqlTokenType.IdentifierQuotedBracket );
            Assert.That( t is SqlTokenIdentifier );
            Assert.That( t.ToString(), Is.EqualTo( "[IN]" ) );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
            Assert.That( tU, Is.SameAs( t ) );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( false );
            Assert.That( tU, Is.Not.SameAs( t ) );
            Assert.That( tU.ToString(), Is.EqualTo( "IN" ) );
            Assert.That( tU.TokenType, Is.EqualTo( SqlTokenType.In ) );

            t = p.ParseWithoutError( "int" ).ElementAt( 0 );
            Assert.That( t.TokenType == SqlTokenType.IntDbType );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
            Assert.That( tU, Is.SameAs( t ) );

            t = p.ParseWithoutError( "[int]" ).ElementAt( 0 );
            Assert.That( t.TokenType == SqlTokenType.IdentifierQuotedBracket );
            Assert.That( t is SqlTokenIdentifier );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
            Assert.That( tU, Is.Not.SameAs( t ) );
            Assert.That( tU.ToString(), Is.EqualTo( "int" ) );
            Assert.That( tU.TokenType == SqlTokenType.IntDbType );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( false );
            Assert.That( tU, Is.Not.SameAs( t ) );
            Assert.That( tU.ToString(), Is.EqualTo( "int" ) );
            Assert.That( tU.TokenType == SqlTokenType.IntDbType );

            t = p.ParseWithoutError( @"""smalliNt""" ).ElementAt( 0 );
            Assert.That( t.TokenType == SqlTokenType.IdentifierQuoted );
            Assert.That( t is SqlTokenIdentifier );
            Assert.That( t.ToString(), Is.EqualTo( @"""smalliNt""" ) );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( true );
            Assert.That( tU, Is.Not.SameAs( t ) );
            Assert.That( tU.ToString(), Is.EqualTo( "smalliNt" ) );
            Assert.That( tU.TokenType == SqlTokenType.SmallIntDbType );
            tU = ((SqlTokenIdentifier)t).RemoveQuoteIfPossible( false );
            Assert.That( tU, Is.Not.SameAs( t ) );
            Assert.That( tU.ToString(), Is.EqualTo( "smalliNt" ) );
            Assert.That( tU.TokenType == SqlTokenType.SmallIntDbType );

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
            Assert.That( r, Is.EqualTo( rewritten ) );
        }

        [Test]
        public void Comments_are_Trivias()
        {
            string s = @"'' -- CancelDate";
            SqlTokenizer p = new SqlTokenizer();
            var tokens = p.Parse( s ).ToArray();
            Assert.That( tokens.Length == 2 );
            Assert.That( tokens[0].TokenType == SqlTokenType.String );
            Assert.That( tokens[0].TrailingTrivias.Count == 2 );
            Assert.That( tokens[0].TrailingTrivias[0].TokenType == SqlTokenType.None );
            Assert.That( tokens[0].TrailingTrivias[0].Text == " " );
            Assert.That( tokens[0].TrailingTrivias[1].TokenType == SqlTokenType.LineComment );
            Assert.That( tokens[0].TrailingTrivias[1].Text == " CancelDate" );
            Assert.That( tokens[1].TokenType == SqlTokenType.EndOfInput );
        }

        [Test]
        public void LineComment_eats_its_prefix_and_LineTermination()
        {
            string s = @"'' -- CancelDate
TOKEN";
            SqlTokenizer p = new SqlTokenizer();
            var tokens = p.Parse( s ).ToArray();
            Assert.That( tokens.Length == 3 );
            Assert.That( tokens[0].TokenType == SqlTokenType.String );
            Assert.That( tokens[0].TrailingTrivias.Count == 2 );
            Assert.That( tokens[0].TrailingTrivias[0].TokenType == SqlTokenType.None );
            Assert.That( tokens[0].TrailingTrivias[0].Text == " " );
            Assert.That( tokens[0].TrailingTrivias[1].TokenType == SqlTokenType.LineComment );
            Assert.That( tokens[0].TrailingTrivias[1].Text == " CancelDate", "No line endings in Text." );
            Assert.That( tokens[1].TokenType == SqlTokenType.IdentifierStandard );
            Assert.That( tokens[1].LeadingTrivias.Count == 0 );
            Assert.That( tokens[1].TrailingTrivias.Count == 0 );
            Assert.That( tokens[2].TokenType == SqlTokenType.EndOfInput );
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

            Assert.That( tokens.Length == 3 );

            Assert.That( tokens[0].LeadingTrivias.Count, Is.EqualTo( 1 ) );
            Assert.That( tokens[0].LeadingTrivias[0].Text, Is.EqualTo( Environment.NewLine ) );
            Assert.That( tokens[0].IsToken( SqlTokenType.Insert ) );
            Assert.That( tokens[0].TrailingTrivias.Count, Is.EqualTo( 2 ) );
            Assert.That( tokens[0].TrailingTrivias[0].Text, Is.EqualTo( " " ) );
            Assert.That( tokens[0].TrailingTrivias[1].Text, Is.EqualTo( " Comment1" ) );

            Assert.That( tokens[1].LeadingTrivias.Count, Is.EqualTo( 4 ) );
            Assert.That( tokens[1].LeadingTrivias[0].Text, Is.EqualTo( string.Empty ) );
            Assert.That( tokens[1].LeadingTrivias[1].Text, Is.EqualTo( " Comment2" ) );
            Assert.That( tokens[1].LeadingTrivias[2].Text, Is.EqualTo( string.Empty ) );
            Assert.That( tokens[1].LeadingTrivias[3].Text, Is.EqualTo( " Comment3 X   " ) );

            Assert.That( tokens[1].IsToken( SqlTokenType.IdentifierStandard ) );
            Assert.That( tokens[1].TrailingTrivias.Count, Is.EqualTo( 0 ) );

            Assert.That( tokens[2].TokenType, Is.EqualTo( SqlTokenType.EndOfInput ) );

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
            Assert.That( t, Is.InstanceOf<SqlTokenIdentifier>().Or.InstanceOf<SqlTokenLiteralString>() );
            if( t is SqlTokenIdentifier )
            {
                var id = (SqlTokenIdentifier)t;
                Assert.That( id.Name, Is.EqualTo( result ) );
            }
            else
            {
                var str = (SqlTokenLiteralString)t;
                Assert.That( str.Value, Is.EqualTo( result ) );
            }
        }
    }
}
