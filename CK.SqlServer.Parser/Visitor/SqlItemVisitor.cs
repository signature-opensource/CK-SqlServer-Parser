using System;
using System.Collections.Generic;
using System.Linq;
using CK.Core;

namespace CK.SqlServer.Parser
{
    public class SqlItemVisitor
    {
        public virtual ISqlNode VisitItem( ISqlNode e )
        {
            return ((SqlNode)e).Accept( this );
        }

        protected List<ISqlNode> VisitItems( IEnumerable<ISqlNode> nodes, ISqlNode prefixToKeep = null, ISqlNode suffixToKeep = null )
        {
            List<ISqlNode> modified = null;
            int i = 0;
            foreach( var a in nodes )
            {
                var ve = VisitItem( a );
                if( !ReferenceEquals( a, ve ) )
                {
                    if( modified == null )
                    {
                        modified = new List<ISqlNode>( i+1 );
                        if( prefixToKeep != null ) modified.Add( prefixToKeep );
                        if( i > 0 )
                        {
                            using( var oldE = nodes.GetEnumerator() )
                            {
                                int j = i;
                                while( --j > 0 ) 
                                {
                                    oldE.MoveNext();
                                    modified.Add( oldE.Current );
                                }
                            }
                        }
                    }
                    modified[i] = ve;
                }
                ++i;
            }
            if( modified != null && suffixToKeep != null ) modified.Add( suffixToKeep );
            return modified;
        }

        protected virtual ISqlNode VisitStandard( ISqlNode e )
        {
            List<ISqlNode> modified = VisitItems( e.ChildrenNodes );
            if( modified == null ) return e;
            return ((SqlNode)e).InternalDoClone( e.LeadingTrivias, modified, e.TrailingTrivias );
        }

        protected virtual ISqlNode VisitTokenStandard( SqlToken e ) => e;

        protected virtual ISqlNode VisitTypeDeclStandard( ISqlUnifiedTypeDecl e ) => e;

        public virtual ISqlNode Visit( SqlNodeExternal e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlGrant e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlOpenXml e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlOpenJSON e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCallParameter e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCallParameterList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlUnnamedStatement e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlNodeList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlEnclosableCommaList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlEnclosedCommaList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlPar e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlTokenLiteralInteger e ) => VisitTokenStandard( e );

        public virtual ISqlNode Visit( SqlTokenError e ) => e;

        public virtual ISqlNode Visit( SqlTokenLiteralBinary e ) => VisitTokenStandard( e );

        public virtual ISqlNode Visit( SqlTokenLiteralFloat e ) => VisitTokenStandard( e );

        public virtual ISqlNode Visit( SqlTokenLiteralDecimal e ) => VisitTokenStandard( e );

        public virtual ISqlNode Visit( SqlTokenLiteralMoney e ) => VisitTokenStandard( e );

        public virtual ISqlNode Visit( SqlTokenLiteralString e ) => VisitTokenStandard( e );

        public virtual ISqlNode Visit( SqlTokenTerminal e ) => VisitTokenStandard( e );

        public virtual ISqlNode Visit( SqlTokenIdentifier e ) => VisitTokenStandard( e );

        public virtual ISqlNode Visit( SqlBasicValue e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlKoCall e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlOverClause e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCollate e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlIf e ) => VisitStandard( e );

        public ISqlNode Visit( SqlCursorDefinition e ) => VisitStandard( e );

        public ISqlNode Visit( SqlIdentifierCommaList e ) => VisitStandard( e );

        public ISqlNode Visit( SqlCursorDefinition92 e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlDeclareCursor e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlBeginTransaction e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlStatementList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlBeginEndBlock e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlTryCatch e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlStatement e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlEmptyStatement e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlReturnStatement e ) => VisitStandard( e );
        
        public virtual ISqlNode Visit( SqlExecuteStatement e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlExecuteStringStatement e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlSetVariable e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlInsertStatement e ) => VisitStandard( e );
        
        public virtual ISqlNode Visit( SqlUpdateStatement e ) => VisitStandard( e );
        
        public virtual ISqlNode Visit( SqlDeleteStatement e ) => VisitStandard( e );
        
        public virtual ISqlNode Visit( SqlMergeStatement e ) => VisitStandard( e );
        
        public virtual ISqlNode Visit( SqlTableValues e ) => VisitStandard( e );
        
        public virtual ISqlNode Visit( SqlMultiCommaList e ) => VisitStandard( e );
        
        public virtual ISqlNode Visit( SqlWithParOptions e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlOptionParOptions e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlWithOptions e ) => VisitStandard( e );

        public virtual ISqlNode Visit( MIUDHeader e ) => VisitStandard( e );
        
        public virtual ISqlNode Visit( IUDTarget e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlOutputClause e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlSetOption e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlGoto e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlWhile e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlRaiserror e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlLabelDefinition e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlView e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlFunctionScalar e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlFunctionInlineTable e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlFunctionTable e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlStoredProcedure e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlTrigger e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlEnclosedIdentifierCommaList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlDeclareVariable e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlVariableDeclarationList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlVariableDeclaration e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCast e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCaseWhenList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlMultiIdentifier e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlOpenDataSource e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlUnaryOperator e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlTypeDeclDecimal e ) => VisitTypeDeclStandard( e );

        public virtual ISqlNode Visit( SqlTypeDeclDateAndTime e ) => VisitTypeDeclStandard( e );

        public virtual ISqlNode Visit( SqlTypeDeclSimple e ) => VisitTypeDeclStandard( e );

        public virtual ISqlNode Visit( SqlTypeDeclWithSize e ) => VisitTypeDeclStandard( e );

        public virtual ISqlNode Visit( SqlTypeDeclUserDefined e ) => VisitTypeDeclStandard( e );

        public virtual ISqlNode Visit( SqlTypeDeclTable e ) => VisitTypeDeclStandard( e );

        public virtual ISqlNode Visit( SqlTypeDeclCursorParameter  e ) => VisitTypeDeclStandard( e );

        public virtual ISqlNode Visit( SqlTypedIdentifier e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlParameter e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlParameterList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlAssign e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCommaList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlBinaryOperator e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlIsNull e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlLike e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlBetween e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlInValues e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCase e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCaseWhenSelector e ) => VisitStandard( e );

        #region Select

        public virtual ISqlNode Visit( SqlSelectStatement e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectSpec e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectColumn e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectColumnList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectHeader e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectInto e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectFrom e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectGroupBy e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectCombine e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectDecorator e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectFor e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlOrderByList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlOrderByItem e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SelectOrderBy e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlNextValueFor e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCTEStatement e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCTENameList e ) => VisitStandard( e );

        public virtual ISqlNode Visit( SqlCTEName e ) => VisitStandard( e );

        #endregion

    }
}
