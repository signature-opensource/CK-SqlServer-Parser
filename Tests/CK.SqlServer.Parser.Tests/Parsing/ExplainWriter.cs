using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser.Tests
{
    public class ExplainWriter : SqlItemVisitor
    {
        readonly StringBuilder Out;

        public ExplainWriter()
        {
            Out = new StringBuilder();
        }

        public static string Write( ISqlNode e )
        {
            ExplainWriter w = new ExplainWriter();
            w.VisitItem( e );
            return w.Out.ToString().NormalizeEOL();
        }

        public override ISqlNode Visit( SqlAssign e )
        {
            Out.Append( '[' );
            VisitItem( e.Left );
            Out.Append( e.AssignT.ToString() );
            VisitItem( e.Right );           
            Out.Append( ']' );
            return e;
        }

        public override ISqlNode Visit( SqlCollate e )
        {
            e.AllTokens.WriteWithoutTrivias( "-", Out );
 	         return e;
        }

        public override ISqlNode Visit( SqlBinaryOperator e )
        {
            Out.Append( '[' );
            VisitItem( e.Left );
            Out.Append( e.Operator.ToString().ToLowerInvariant() );
            VisitItem( e.Right );
            Out.Append( ']' );
            return e;
        }

        public override ISqlNode Visit( SqlTokenIdentifier e )
        {
            WriteIdentifier( e );
            return e;
        }

        public override ISqlNode Visit( SqlMultiIdentifier e )
        {
            WriteIdentifier( e );
            return e;
        }

        void WriteIdentifier( ISqlIdentifier id )
        {
            id.AllTokens.WriteWithoutTrivias( String.Empty, Out );
            //Out.Append( String.Join( ".", id.Select( n => n.Name ) ) );
        }

        public override ISqlNode Visit( SqlIf e )
        {
            Out.Append( "if[" );
            VisitItem( e.Condition );
            Out.Append( "]then[" );
            VisitItem( e.Then );
            Out.Append( ']' );
            if( e.HasElse )
            {
                Out.Append( "else[" );
                VisitItem( e.Else );
                Out.Append( ']' );
            }
            return e;
        }

        public override ISqlNode Visit( SqlStatement e )
        {
            Out.Append( '<' );
            VisitItem( e.Content );
            Out.Append( '>' );
            return e;
        }

        public override ISqlNode Visit( SqlEmptyStatement e )
        {
            Out.Append( "<empty statement>" );
            return e;
        }

        public override ISqlNode Visit( SqlUnaryOperator e )
        {
            Out.Append( e.Operator.ToString().ToLowerInvariant() ).Append( '[' );
            VisitItem( e.Right );
            Out.Append( ']' );
            return e;
        }

        public override ISqlNode Visit( SqlPar e )
        {
            Out.Append( "(%" );
            VisitItem( e.Content );
            Out.Append( "%)" );
            return e;
        }

        public override ISqlNode Visit( SqlCast e )
        {
            Out.Append( '(' );
            VisitItem( e.Type );
            Out.Append( ')' );
            Out.Append( '[' );
            VisitItem( e.Expression );
            Out.Append( ']' );
            return e;
        }

        public override ISqlNode Visit( SqlEnclosableCommaList e )
        {
            Out.Append( '(' );
            bool one = false;
            foreach( var item in e )
            {
                if( one ) Out.Append( ',' );
                one = true;
                VisitItem( item );
            }
            Out.Append( ')' );
            return e;
        }

        public override ISqlNode Visit( SqlIsNull e )
        {
            Out.Append( e.IsNotNull ? "IsNotNull(" : "IsNull(" );
            VisitItem( e.Left );
            Out.Append( ')' );
            return e;
        }

        public override ISqlNode Visit( SqlBetween e )
        {
            Out.Append( e.IsNotBetween ? "NotBetween(" : "Between(" );
            VisitItem( e.Left );
            Out.Append( ',' );
            VisitItem( e.Start );
            Out.Append( ',' );
            VisitItem( e.Stop );
            Out.Append( ')' );
            return e;
        }

        public override ISqlNode Visit( SqlCase e )
        {
            Out.Append( "case" );
            if( e.IsSimpleCase )
            {
                Out.Append( '(' );
                VisitItem( e.Expression );
                Out.Append( ')' );
            }
            VisitItem( e.WhenList );
            if( e.HasElse )
            {
                Out.Append( ':' );
                VisitItem( e.ElseExpression );
            }
            return e;
        }

        public override ISqlNode Visit( SqlCaseWhenSelector e )
        {
            Out.Append( ':' );
            VisitItem( e.Expression );
            Out.Append( "=>" );
            VisitItem( e.Value );
            return e;
        }

        public override ISqlNode Visit( SqlLike e )
        {
            Out.Append( e.IsNotLike ? "NotLike(" : "Like(" );
            VisitItem( e.Left );
            Out.Append( ',' );
            VisitItem( e.Pattern );
            if( e.HasEscape )
            {
                Out.Append( ',' );
                Out.Append( e.EscapeChar.LiteralValue );
            } 
            Out.Append( ')' );
            return e;
        }

        public override ISqlNode Visit( SqlInValues e )
        {
            Out.Append( e.IsNotIn ? "NotIn(" : "In(" );
            VisitItem( e.Left );
            Out.Append( '∈' );
            VisitItem( e.Values );
            Out.Append( ')' );
            return e;
        }

        public override ISqlNode Visit( SqlKoCall e )
        {
            Out.Append( "call:" );
            VisitItem( e.FunName );
            Out.Append( '(' );
            bool already = false;
            foreach( var b in e.Parameters )
            {
                if( already ) Out.Append( ',' );
                VisitItem( b );
                already = true;
            }
            Out.Append( ')' );
            if( e.OverClause != null ) Visit( e.OverClause );
            return e;
        }

        public override ISqlNode Visit( SqlOverClause e )
        {
            Out.Append( "OVER[" );
            VisitItem( e.OverContent );
            Out.Append( ']' );
            return e;
        }
        
        public override ISqlNode Visit( SelectSpec e )
        {
            Out.Append( '[' );
            VisitItem( e.Header );
            Out.Append( "-" );
            VisitItem( e.Columns );
            if( e.IntoClause != null ) VisitItem( e.IntoClause );
            if( e.FromClause != null ) VisitItem( e.FromClause );
            if( e.GroupByClause != null ) VisitItem( e.GroupByClause );
            Out.Append( ']' );
            return e;
        }

        public override ISqlNode Visit( SelectHeader e )
        {
            e.AllTokens.WriteWithoutTrivias( "-", Out );
            return e;
        }

        public override ISqlNode Visit( SelectColumnList e )
        {
            Out.Append( "(" );
            bool atLeastOne = false;
            foreach( SelectColumn c in e )
            {
                if( atLeastOne ) Out.Append( "," );
                else atLeastOne = true;
                VisitItem( c );
            }
            Out.Append( ")" );
            return e;
        }

        public override ISqlNode Visit( SelectColumn e )
        {
            if( e.ColumnName != null )
            {
                VisitItem( e.ColumnName );
                Out.Append( '-' ).Append( e.AsOrEqualT.ToString() ).Append( '-' );
            }
            VisitItem( e.Definition );
            return e;
        }

        public override ISqlNode Visit( SelectInto e )
        {
            Out.Append( "-into[" );
            WriteIdentifier( e.TableName );
            Out.Append( "]" );
            return e;
        }

        public override ISqlNode Visit( SelectFrom e )
        {
            Out.Append( "-from[" );
            VisitItem( e.Content );
            Out.Append( "]" );
            return e;
        }

        public override ISqlNode Visit( SelectGroupBy e )
        {
            Out.Append( "-groupBy[" );
            VisitItem( e.GroupExpression );
            Out.Append( "]" );
            if( e.HavingExpression != null )
            {
                Out.Append( "-having[" );
                VisitItem( e.HavingExpression );
                Out.Append( "]" );
            }
            return e;
        }

        public override ISqlNode Visit( SelectOrderBy e )
        {
            Out.Append( "OrderBy(" );
            VisitItem( e.SelectNode );
            Out.Append( "," );
            VisitItem( e.OrderByColumns );
            if( e.OffsetClause != null )
            {
                Out.Append( "," );
                VisitItem( e.OffsetClause );
            }
            Out.Append( ")" );
            return e;
        }

        public override ISqlNode Visit( SqlOrderByList e )
        {
            Out.Append( "(" );
            bool atLeastOne = false;
            foreach( SqlOrderByItem c in e )
            {
                if( atLeastOne ) Out.Append( "," );
                else atLeastOne = true;
                VisitItem( c );
            }
            Out.Append( ")" );
            return e;
        }

        public override ISqlNode Visit( SqlOrderByItem e )
        {
            VisitItem( e.Definition );
            if( e.AscOrDescT != null )
            {
                Out.Append( "-" );
                Out.Append( e.AscOrDescT.Name );
            }
            return e;
        }

        public override ISqlNode Visit( SelectOrderByOffset e )
        {
            Out.Append( "offset:" );
            VisitItem( e.OffsetExpression );
            if( e.HasFetchClause )
            {
                Out.Append( ",fetch:" );
                VisitItem( e.FetchExpression );
            }
            return e;
        }


        public override ISqlNode Visit( SelectFor e )
        {
            Out.Append( "For(" );
            VisitItem( e.ForExpression );
            Out.Append( "," );
            VisitItem( e.ForExpression );
            Out.Append( ")" );
            return e;
        }

        public override ISqlNode Visit( SelectCombine e )
        {
            Out.Append( '[' );
            VisitItem( e.LeftNode );
            e.OperatorT.AllTokens.WriteWithoutTrivias( "-", Out );
            VisitItem( e.RightNode );
            Out.Append( ']' );
            return e;
        }

    }
}
