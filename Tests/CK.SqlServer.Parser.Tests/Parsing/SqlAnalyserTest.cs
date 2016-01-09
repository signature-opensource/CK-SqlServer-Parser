using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Xml.Linq;

namespace CK.SqlServer.Parser.Tests
{
    [TestFixture]
    [Category( "SqlAnalyser" )]
    public class SqlAnalyserTest
    {
        [Test]
        public void ParseStoredProcedureInputOutput()
        {
            CheckStatement<SqlStoredProcedure>( "sStoredProcedureInputOutput.sql", sp =>
            {
                Assert.That( sp.Name.Identifiers[0].ToString(), Is.EqualTo( "CK" ) );
                Assert.That( sp.Name.Identifiers[1].ToString(), Is.EqualTo( "sStoredProcedureInputOutput" ) );
                Assert.That( sp.Name.ToString(), Is.EqualTo( "CK.sStoredProcedureInputOutput" ) );

                Assert.That( sp.Parameters[0].IsOutput, Is.False );
                Assert.That( sp.Parameters[0].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[0].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[0].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[0].Variable.Identifier.Name, Is.EqualTo( "@p1" ) );
                Assert.That( sp.Parameters[0].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Int ) );
                Assert.That( sp.Parameters[0].Variable.TypeDecl.SyntaxSize, Is.EqualTo( -2 ), "Size does not apply." );

                Assert.That( sp.Parameters[1].IsOutput, Is.False );
                Assert.That( sp.Parameters[1].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[1].DefaultValue, Is.Not.Null );
                Assert.That( sp.Parameters[1].DefaultValue.ToString(), Is.EqualTo( "0" ) );
                Assert.That( sp.Parameters[1].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[1].Variable.Identifier.Name, Is.EqualTo( "@p2" ) );
                Assert.That( sp.Parameters[1].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.TinyInt ) );

                Assert.That( sp.Parameters[2].IsOutput, Is.True );
                Assert.That( sp.Parameters[2].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[2].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[2].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[2].Variable.Identifier.Name, Is.EqualTo( "@p3" ) );
                Assert.That( sp.Parameters[2].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.SmallInt ) );

                Assert.That( sp.Parameters[3].IsOutput, Is.False );
                Assert.That( sp.Parameters[3].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[3].DefaultValue.ToString(), Is.EqualTo( "N'Murfn...'" ) );
                Assert.That( sp.Parameters[3].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[3].Variable.Identifier.Name, Is.EqualTo( "@p4" ) );
                Assert.That( sp.Parameters[3].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.NVarChar ) );
                Assert.That( sp.Parameters[3].Variable.TypeDecl.SyntaxSize, Is.EqualTo( 50 ) );

                Assert.That( sp.Parameters[4].IsOutput, Is.True );
                Assert.That( sp.Parameters[4].IsInputOutput, Is.True );
                Assert.That( sp.Parameters[4].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[4].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[4].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[4].Variable.Identifier.Name, Is.EqualTo( "@p5" ) );
                Assert.That( sp.Parameters[4].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.VarChar ) );
                Assert.That( sp.Parameters[4].Variable.TypeDecl.SyntaxSize, Is.EqualTo( -1 ), "Size is max." );

                Assert.That( sp.Parameters[5].IsOutput, Is.True );
                Assert.That( sp.Parameters[5].IsInputOutput, Is.True );
                Assert.That( sp.Parameters[5].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[5].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[5].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[5].Variable.Identifier.Name, Is.EqualTo( "@p6" ) );
                Assert.That( sp.Parameters[5].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Char ) );
                Assert.That( sp.Parameters[5].Variable.TypeDecl.SyntaxSize, Is.EqualTo( 0 ), "Size is undefined." );

                Assert.That( sp.Parameters[6].IsOutput, Is.True );
                Assert.That( sp.Parameters[6].IsInputOutput, Is.False, "--input behind the comma..." );
                Assert.That( sp.Parameters[6].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[6].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[6].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[6].Variable.Identifier.Name, Is.EqualTo( "@p7" ) );
                Assert.That( sp.Parameters[6].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Xml ) );
                Assert.That( sp.Parameters[6].Variable.TypeDecl.SyntaxSize, Is.EqualTo( -2 ), "Size does not apply." );

                Assert.That( sp.Parameters[7].IsOutput, Is.True );
                Assert.That( sp.Parameters[7].IsInputOutput, Is.True, "-- input on the line above." );
                Assert.That( sp.Parameters[7].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[7].DefaultValue, Is.Null );
                Assert.That( sp.Parameters[7].Variable.Identifier.IsVariable, Is.True );
                Assert.That( sp.Parameters[7].Variable.Identifier.Name, Is.EqualTo( "@p8" ) );
                Assert.That( sp.Parameters[7].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.SmallDateTime ) );
                Assert.That( sp.Parameters[7].Variable.TypeDecl.SyntaxSize, Is.EqualTo( -2 ), "Size does not apply." );

                Assert.That( sp.Parameters[8].IsOutput, Is.False );
                Assert.That( sp.Parameters[8].IsInputOutput, Is.False );
                Assert.That( sp.Parameters[8].IsReadOnly, Is.False );
                Assert.That( sp.Parameters[8].DefaultValue.IsVariable, Is.False );
                Assert.That( sp.Parameters[8].DefaultValue.IsNull, Is.True );
                Assert.That( sp.Parameters[8].DefaultValue.IsLiteral, Is.False );

                Assert.That( sp.Header.ToStringCompact(), Is.EqualTo( "procedure CK.sStoredProcedureInputOutput @p1 int, @p2 tinyint = 0, @p3 smallint output, @p4 nvarchar(50)=N'Murfn...', @p5 varchar(max) /*input*/output, @p6 char /*input*/output, @p7 Xml output, @p8 smalldatetime /*input*/output, @p9 smalldatetime = null" ) );
            } );
        }

        [Test]
        public void SelectUnionAndOrderBy()
        {
            {
                var sc1 = "(((((select name from sys.tables where X))))) order by name";
                Check( sc1, "OrderBy([select-(name)-from[sys.tables]-where[X]],(name))" );
            }
            {
                // This is not syntaxically valid.
                var sc1 = "((select name from sys.tables where X) order by name) for xml auto";
                Check( sc1, "For(OrderBy([select-(name)-from[sys.tables]-where[X]],(name)),¤{xml-auto}¤)" );
            }
            {
                var sc1 = @"((((
	                            (select name from sys.tables where name like '%a%')
                            union
	                            (((select 'u'+name from sys.tables where name like '%a%')))
                            ))))
                            order by name desc
                            for xml auto";
                var sc2 = @"select name from sys.tables where name like '%a%'
                            union
                            select 'u'+name from sys.tables where name like '%a%'
                            order by name desc
                            for xml auto";
                var c = @"For(
                                OrderBy(
                                        [
                                                [select-(name)-from[sys.tables]-where[Like(name,'%a%')]]
                                             union
                                                [select-(['u'+name])-from[sys.tables]-where[Like(name,'%a%')]]
                                        ], (name-desc)
                                     ), ¤{xml-auto}¤
                              )";

                Check( sc1, c );
                Check( sc2, c );
            }
        }

        [Test]
        public void ParseExpression01()
        {
            Check( "a", "a" );
            Check( "457", "457" );
            Check( " ( ( 457 ) ) ", "(%(%457%)%)" );
            Check( "(a)", "(%a%)" );
            Check( "*", "*" );
            Check( @"(""in"")", @"(%""in""%)" );
            Check( @"([is])", @"(%[is]%)" );

            Check( "a-b", "[a-b]" );
            Check( "(a-b)", "(%[a-b]%)" );
            Check( "( ( ( (a-b)   ))  )", "(%(%(%(%[a-b]%)%)%)%)" );

            Check( "(~2)", "(%~[2]%)" );
            Check( "~ 0 * 1 = (~2) * 3", "[[~[0]*1]=[(%~[2]%)*3]]" );
            Check( "0 + 1  * 2 >= ~3 / 4 + 1", "[[0+[1*2]]>=[[~[3]/4]+1]]" );
            Check( "1 = 1 and 0 = 0 and 2 = 2", "[[[1=1]and[0=0]]and[2=2]]" );
            Check( "1 = 1 and 0 = 0 or 2 = 2", "[[[1=1]and[0=0]]or[2=2]]" );
            Check( "1 = 1 or 1 = 1 and 0 = 1", "[[1=1]or[[1=1]and[0=1]]]" );
            Check( "(a+(b)+c)", "(%[[a+(%b%)]+c]%)" );
            Check( "(a >= b)", "(%[a>=b]%)" );
            Check( "(1 = 1 or 1 = 1) and 0 = 1", "[(%[[1=1]or[1=1]]%)and[0=1]]" );
            Check( "not 1 = 1 or 1 = 1", "[not[[1=1]]or[1=1]]" );
            Check( "not (1 = 1 or 1 = 1)", "not[(%[[1=1]or[1=1]]%)]" );
            Check( "a-b, a*8+3, (($78))", "{[a-b],[[a*8]+3],(%(%$78%)%)}" );
       }

        [Test]
        public void ParseIsNull()
        {
            Check( "~@i is null", "IsNull(~[@i])" );
            Check( "~@i is not null", "IsNotNull(~[@i])" );
            Check( "~@i * 8 is null", "IsNull([~[@i]*8])" );
            Check( "~@i * (a*b) is not null", "IsNotNull([~[@i]*[a*b]])" );
            Check( "not ~@i is null", "not[IsNull(~[@i])]" );
            Check( "not ~@i is null and 1=0", "[not[IsNull(~[@i])]and[1=0]]" );

            Check( "not ((((~@i) is null)))", "not[IsNull(~[@i])]" );
            Check( "not ((((~@i) is not null)))", "not[IsNotNull(~[@i])]" );
        }

        [Test]
        public void ParseLike()
        {
            Check( "'text' like @i+@j and 1 = 1", "[Like('text',[@i+@j])and[1=1]]" );
            Check( "not 'text' like @i+@j and 1 = 1", "[not[Like('text',[@i+@j])]and[1=1]]" );
            Check( "'text' not like @i+@j or 1 = 1", "[NotLike('text',[@i+@j])or[1=1]]" );
            Check( "not 'text' not like @i+'p'+@j and 1 = 1", "[not[NotLike('text',[[@i+'p']+@j])]and[1=1]]" );

            Check( "'text' not like @i+@j escape N'e'", "NotLike('text',[@i+@j],N'e')" );
            Check( "'text' like @i+@j escape N'e' and 1 = 1", "[Like('text',[@i+@j],N'e')and[1=1]]" );

            Check( "(('text' like @i+@j escape 'a')) and 1 = 1", "[Like('text',[@i+@j],'a')and[1=1]]" );
        }

        [Test]
        public void ParseBetween()
        {
            Check( "4 + 5 * 8 between 4 / 8 * 9 and 457 or 1=1", "[Between([4+[5*8]],[[4/8]*9],457)or[1=1]]" );
            Check( "4 + 5 * 8 between 4 / 8 * 9 and 457 and 1=1", "[Between([4+[5*8]],[[4/8]*9],457)and[1=1]]" );
            Check( "4 + 5 * 8 between 4 / 8 * 9 and 457 = 4+7", "[Between([4+[5*8]],[[4/8]*9],457)=[4+7]]" );
            Check( "4 + 5 * 8 not between 4 / 8 * 9 and 457 = 4+7", "[NotBetween([4+[5*8]],[[4/8]*9],457)=[4+7]]" );
            Check( "not 4 + 5 * 8 not between 4 / 8 * 9 and 457 or 1 = /*comment 4 Fun*/0", "[not[NotBetween([4+[5*8]],[[4/8]*9],457)]or[1=0]]" );
            Check( "(((4 + 5 * 8 not between 4 / 8 * 9 and 457))) = 4+7", "[(%(%(%NotBetween([4+[5*8]],[[4/8]*9],457)%)%)%)=[4+7]]" );
        }

        [Test]
        public void ParseIn()
        {
            Check( "@i in ( 1, 2, 3 )", "In(@i∈{1,2,3})" );
            Check( "@i not in ( 1, 2 )", "NotIn(@i∈{1,2})" );
            Check( "2*~5 not in ( 7 )", "NotIn([2*~[5]]∈{7})" );
            Check( "not 2*~5 not in ( 7 )", "not[NotIn([2*~[5]]∈{7})]" );
            Check( "not 2*~5 not in ( 7 ) or 1=1", "[not[NotIn([2*~[5]]∈{7})]or[1=1]]" );
            Check( "3 in (4+5,6,select Power from CK.tShmurtz) or 1=1", "[In(3∈{[4+5],6,[select-(Power)-from[CK.tShmurtz]]})or[1=1]]" );
            Check( "((((@i in ( 1, 2, 3 )))))", "In(@i∈{1,2,3})" );
        }

        [Test]
        public void ParseKoCall()
        {
            Check( "3 + AnyCall()", "[3+call:AnyCall()]" );
            Check( "3 + AnyCall(5, N'kjkj'+8)", "[3+call:AnyCall(5,[N'kjkj'+8])]" );
            Check( "3 < all (select Power from dbo.tNuclearPlant)", "[3<call:all([select-(Power)-from[dbo.tNuclearPlant]])]" );
            Check( "(3 < all (select Power from dbo.tNuclearPlant))", "[3<call:all([select-(Power)-from[dbo.tNuclearPlant]])]" );
            Check( "3 + ((AnyCall(5, N'kjkj'+8)))", "[3+call:AnyCall(5,[N'kjkj'+8])]" );
        }

        [Test]
        public void ParseSimpleCase()
        {
            Check( "case @i when 0 then 1 end", "case(@i):0=>1" );
            Check( "case @i+7 when 0.2 then 1.6 when 3+@i then null end", "case([@i+7]):0.2=>1.6:[3+@i]=>null" );
            Check( "case 0 when 0 then 1 else 2 end", "case(0):0=>1:2" );
            Check( "case 0 when 0 then 1 else @i*8 end", "case(0):0=>1:[@i*8]" );
        }

        [Test]
        public void ParseSearchCase()
        {
            Check( "case when 0=0 then 1 end", "case:[0=0]=>1" );
            Check( "case when 0>1 then 1 when 0<1 then null end", "case:[0>1]=>1:[0<1]=>null" );
            Check( "case when (0<>5) then 4 else ((5/8)) end", "case:(%[0<>5]%)=>4:(%(%[5/8]%)%)" );
        }

        [Test]
        public void ParseSelectAssign()
        {
            Check( "select @hid = hierarchyid::GetRoot(), @i = 87/7", "[select-(@hid-=-call:hierarchyid::GetRoot(),@i-=-[87/7])]" );
        }

        [Test]
        public void ParseIf01()
        {
            var ifS = TestHelper.ParseOneStatementAndCheckString<SqlIf>( @"
                        if @i is null print '1';
                        else print 2, 9, 'toto';" );

            var x = XElement.Parse( @"
                <Sql EType=""SqlIf"">
                    <T>if @i is null print '1'; else print 2, 9, 'toto';</T>
                    <Condition EType=""SqlIsNull"">
                        <T>@i is null</T>
                    </Condition>
                    <ThenStatement EType=""SqlUnmodeledStatement"">

                    </ThenStatement>
                    <ElseStatement EType=""SqlUnmodeledStatement"">

                    </ElseStatement>
                </Sql>" );

            Assert.That( ifS.ToXml().ToString(), Is.EqualTo( x.ToString() ) );
            Assert.That( XNode.DeepEquals( ifS.ToXml(), x ) );
        }

        [Test]
        public void ParseIf02()
        {
            var ifS = TestHelper.ParseOneStatementAndCheckString<SqlIf>( @"if exists(select t.* from sys.tables t) print N'OK';" );
            Assert.That( ExplainWriter.Write( ifS ), Is.EqualTo( "if[call:exists([select-(t.*)-from[¤{sys.tables-t}¤]])]then[<¤{print-N'OK'}¤>]" ) );
        }

        private static void Check( string text, string explained, string textAutoCorrected = null )
        {
            text = text.NormalizeEOL();
            explained = explained.NormalizeEOL();
            ISqlNode e;
            var r = SqlAnalyser.Parse( out e, ParseMode.OneExpression, text );
            Assert.That( r.IsError, Is.False, r.ToString() );
            Assert.That( ExplainWriter.Write( e ), Is.EqualTo( Regex.Replace( explained, @"\s*", String.Empty ) ) );
            Assert.That( e.ToString( true ).NormalizeEOL(), Is.EqualTo( textAutoCorrected ?? text ) );
        }

        [Test]
        public void Parse_OpenJSON_select()
        {
            string s = @"SELECT Number, Customer, Date, Quantity
                         FROM OPENJSON (@JSalestOrderDetails, '$.OrdersArray')
                         WITH (
                                Number varchar(200), 
                                Date datetime,
                                Customer varchar(200),
                                Quantity int
                         ) AS OrdersArray";
            ISqlNode e;
            var r = SqlAnalyser.Parse( out e, ParseMode.OneExpression, s );
            Assert.That( r.IsError, Is.False, r.ToString() );
            Assert.That( e is ISelectSpecification );
        }

        [Test]
        public void ParseStoredProcedureWithoutTerminator()
        {
            var sp = ReadStatement<SqlStoredProcedure>( "sProcWithoutTerminator.sql" );

            Assert.That( sp.Name.ToString( true ), Is.EqualTo( "sProcWithoutTerminator" + Environment.NewLine ) );
            Assert.That( sp.Parameters[0].IsOutput, Is.False );
            Assert.That( sp.Parameters[0].IsReadOnly, Is.False );
            Assert.That( sp.Parameters[0].DefaultValue, Is.Null );
            Assert.That( sp.Parameters[0].Variable.Identifier.IsVariable, Is.True );
            Assert.That( sp.Parameters[0].Variable.Identifier.Name, Is.EqualTo( "@P" ) );
            Assert.That( sp.Parameters[0].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Int ) );
            Assert.That( sp.Body.Count, Is.EqualTo( 1 ) );
        }

        [Test]
        public void ParseFunctionAclGrantLevel()
        {
            CheckStatement<SqlFunctionScalar>( "fAclGrantLevel.sql", f =>
            {
                Assert.That( f.Name.ToString(), Is.EqualTo( "CK.fAclGrantLevel" ) );
                Assert.That( f.Parameters[0].IsOutput, Is.False );
                Assert.That( f.Parameters[0].IsReadOnly, Is.False );
                Assert.That( f.Parameters[0].DefaultValue, Is.Null );
                Assert.That( f.Parameters[0].Variable.Identifier.IsVariable, Is.True );
                Assert.That( f.Parameters[0].Variable.Identifier.Name, Is.EqualTo( "@ActorId" ) );
                Assert.That( f.Parameters[0].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Int ) );
                Assert.That( f.Parameters.Count, Is.EqualTo( 2 ) );
                Assert.That( f.Parameters[1].Variable.Identifier.Name, Is.EqualTo( "@AclId" ) );
                Assert.That( f.Parameters[1].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Int ) );
                Assert.That( f.ReturnsT, Is.Not.Null );
                Assert.That( f.ReturnedType.DbType, Is.EqualTo( SqlDbType.TinyInt ) );
                Assert.That( f.BodyStatements.Count, Is.EqualTo( 1 ) );
                Assert.That( f.BodyStatements[0], Is.InstanceOf<SqlReturnStatement>() );
                SqlReturnStatement r = (SqlReturnStatement)f.BodyStatements[0];
                SqlKoCall isNull = (SqlKoCall)r.Value;
                SqlKoCall isNull2 = (SqlKoCall)isNull.Parameters[1];
                SqlTokenLiteralInteger zero = (SqlTokenLiteralInteger)isNull2.Parameters[1];
                Assert.That( zero.LiteralValue, Is.EqualTo( "0" ) );
            } );
        }

        [Test]
        public void ParseFunctionInlineTable()
        {
            CheckStatement<SqlFunctionInlineTable>( "fReadThings.sql", f =>
                {
                    Assert.That( f.Name.ToString(), Is.EqualTo( "CK.fReadThings" ) );
                    Assert.That( f.Parameters[0].IsOutput, Is.False );
                    Assert.That( f.Parameters[0].IsReadOnly, Is.False );
                    Assert.That( f.Parameters[0].DefaultValue, Is.Null );
                    Assert.That( f.Parameters[0].Variable.Identifier.IsVariable, Is.True );
                    Assert.That( f.Parameters[0].Variable.Identifier.Name, Is.EqualTo( "@ActorId" ) );
                    Assert.That( f.Parameters[0].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Int ) );
                    Assert.That( f.Parameters.Count, Is.EqualTo( 2 ) );
                    Assert.That( f.Parameters[1].Variable.Identifier.Name, Is.EqualTo( "@AclId" ) );
                    Assert.That( f.Parameters[1].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Int ) );
                    Assert.That( f.ReturnsT, Is.Not.Null );
                    Assert.That( f.Select, Is.Not.Null );
                } );
        }

        [Test]
        public void ParseStoredProcedureWithOptions()
        {
            //
            // create procedure sWithOptions
            //    with recompile, execute as owner
            // as
            //    select * from sys.tables;
            //    return 0;
            //
            CheckStatement<SqlStoredProcedure>( "sWithOptions.sql", sp =>
                {
                    Assert.That( sp.Name.ToString( true ), Is.EqualTo( "sWithOptions" + Environment.NewLine ) );
                    Assert.That( sp.Parameters.Count, Is.EqualTo( 0 ) );
                    Assert.That( sp.HasOptions );
                    Assert.That( sp.Options.ChildrenNodes.Count(), Is.EqualTo( 4 ), "[with] [recompile] [,] [execute as owner]" );
                    Assert.That( sp.Options.AllTokens.ToStringWithoutTrivias( "|" ), Is.EqualTo( "with|recompile|,|execute|as|owner" ) );
                    Assert.That( sp.Body.Count, Is.EqualTo( 2 ).Or.EqualTo( 3 ), "Two statements (select and return) but..." );
                    Assert.That( sp.Body.Count == 2 || sp.Body[2] is SqlEmptyStatement, "...when ';' is added, it is a third empty statement." );

                    Assert.That( sp.Header.ToStringCompact(), Is.EqualTo( "procedure sWithOptions with recompile, execute as owner" ) );
                } );
        }

        [Test]
        public void ParseStrangeStoredProcedure()
        {
            CheckStatement<SqlStoredProcedure>( "sStrange.sql", sp =>
                {
                    Assert.That( sp.Name.ToString( true ), Is.EqualTo( "sStrange -- funny one" + Environment.NewLine ) );
                    Assert.That( sp.Parameters, Is.Empty );
                    Assert.That( sp.Body.Count, Is.EqualTo( 5 ) );
                    Assert.That( sp.Header.ToStringCompact(), Is.EqualTo( "procedure sStrange" ) );
                } );
        }

        [Test]
        public void ParseStoredProcedure01()
        {
            CheckStatement<SqlStoredProcedure>( "sStoredProcedure01.sql", sp =>
                {
                    Assert.That( sp.Name.ToString( true ), Is.EqualTo( "CKCore.sErrorRethrow" + Environment.NewLine ) );
                    Assert.That( sp.Parameters[0].IsOutput, Is.False );
                    Assert.That( sp.Parameters[0].IsReadOnly, Is.False );
                    Assert.That( sp.Parameters[0].DefaultValue, Is.Null );
                    Assert.That( sp.Parameters[0].Variable.Identifier.IsVariable, Is.True );
                    Assert.That( sp.Parameters[0].Variable.Identifier.Name, Is.EqualTo( "@ProcId" ) );
                    Assert.That( sp.Parameters[0].Variable.TypeDecl.DbType, Is.EqualTo( SqlDbType.Int ) );
                    Assert.That( sp.HasBeginEnd );
                    Assert.That( sp.HasOptions, Is.False );
                    Assert.That( sp.Body.Count, Is.EqualTo( 2 ) );
                    Assert.That( sp.Header.ToStringCompact(), Is.EqualTo( "procedure CKCore.sErrorRethrow(@ProcId int)" ) );
                } );
        }

        [Test]
        public void ParseStoredProcedure02()
        {
            CheckStatement<SqlStoredProcedure>( "sStoredProcedure02.sql", sp =>
                {
                    Assert.That( sp.Name.ToString( true ), Is.EqualTo( "CK.sResDataStringSet -- Merge inside!" + Environment.NewLine ) );
                    Assert.That( sp.Parameters.Count, Is.EqualTo( 2 ) );
                    Assert.That( sp.HasBeginEnd );
                    Assert.That( sp.HasOptions, Is.False );
                    Assert.That( sp.Body.Count, Is.EqualTo( 1 ), "Unmodeled." );
                    Assert.That( sp.Header.ToStringCompact(), Is.EqualTo( "procedure CK.sResDataStringSet(@ResId int, @Val nvarchar(400))" ) );
                } );
        }

        [Test]
        public void ParseStoredProcedure03()
        {
            CheckStatement<SqlStoredProcedure>( "sStoredProcedure03.sql", sp =>
                {
                    Assert.That( sp.Name.ToString(), Is.EqualTo( "InvBack.sOfferCreate" ) );
                    Assert.That( sp.HasBeginEnd );
                    Assert.That( sp.HasOptions, Is.False );
                    Assert.That( sp.Parameters.Count, Is.EqualTo( 7 ) );
                    Assert.That( sp.Header.ToStringCompact(), Is.EqualTo( "proc InvBack.sOfferCreate(@ActorId int, @Title nvarchar(256), @ProjectName nvarchar(256), @ClientId int, @ContactId int, @CompanyLocationId int, @OfferIdResult int output)" ) );
                } );
        }

        [Test]
        public void ParseStoredProcedure_GroupRemoveAllUsers()
        {
            var sp = ReadStatement<SqlStoredProcedure>( "sGroupRemoveAllUsers.sql" );

            Assert.That( sp.Name.ToString(), Is.EqualTo( "CK.sGroupRemoveAllUsers" ) );
            Assert.That( sp.Parameters.Count, Is.EqualTo( 2 ) );
            Assert.That( sp.Body.Count, Is.GreaterThan( 1 ) );
        }

        [Test]
        public void ParseStoredProcedure_cursor_usage()
        {
            var sp = ReadStatement<SqlStoredProcedure>( "cursor_usage.sql" );

            Assert.That( sp.Name.ToString(), Is.EqualTo( "cursor_usage" ) );
            Assert.That( sp.Parameters.Count, Is.EqualTo( 0 ) );
            Assert.That( sp.Body.Count, Is.GreaterThan( 1 ) );
        }

        [DebuggerStepThrough]
        internal static T CheckStatement<T>( string fileName, Action<T> check ) where T : ISqlStatement
        {
            string text = TestHelper.LoadTextFromParsingScripts( fileName );
            T s = TestHelper.ParseOneStatementAndCheckString<T>( text, false );
            check( s );
            s = TestHelper.ParseOneStatementAndCheckString<T>( text, true );
            check( s );
            return s;
        }

        [DebuggerStepThrough]
        internal static T ReadStatement<T>( string fileName, bool addSemiColon = false ) where T : ISqlStatement
        {
            string text = TestHelper.LoadTextFromParsingScripts( fileName );
            return TestHelper.ParseOneStatementAndCheckString<T>( text, addSemiColon );
        }

    }
}
