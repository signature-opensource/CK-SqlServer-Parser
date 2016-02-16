using System;
using System.Collections.Generic;
using System.Linq;
using CK.Core;
using System.Diagnostics;

namespace CK.SqlServer.Parser
{
    public class SqlNodeVisitor
    {
        /// <summary>
        /// Main entry point that calls the protected virtual <see cref="VisitItem"/>.
        /// All other visit methods are protected.
        /// </summary>
        /// <param name="root">Root node to visit.</param>
        /// <returns>The root node or a transformed one.</returns>
        public virtual ISqlNode VisitRoot( ISqlNode root ) => VisitItem( root );

        protected virtual ISqlNode VisitItem( ISqlNode e )
        {
            return ((SqlNode)e).Accept( this );
        }
        /// <summary>
        /// Visits childre nodes and handles any modification that may occur on visited children.
        /// This virtual method can be rerouted to <see cref="VisitStandardReadOnly"/> if there is no
        /// mutations planned or if the mutations should be ignored.
        /// </summary>
        /// <param name="e">The visited node.</param>
        /// <returns>The resulting node.</returns>
        protected virtual ISqlNode VisitStandard( ISqlNode e )
        {
            IList<ISqlNode> content = e.GetRawContent();
            return VisitRawContent( content ) ? e.SetRawContent( content ) : e;
        }

        /// <summary>
        /// Helper method that ignores any mutation of the visited children.
        /// </summary>
        /// <param name="e">The visited node.</param>
        /// <returns>The unchanged visited node.</returns>
        protected virtual ISqlNode VisitStandardReadOnly( ISqlNode e )
        {
            foreach( var c in e.ChildrenNodes ) VisitItem( c );
            return e;
        }

        bool VisitRawContent( IList<ISqlNode> nodes )
        {
            bool isModified = false;
            List<ISqlNode> dynamicList = nodes as List<ISqlNode>;
            if( dynamicList != null )
            {
                for( int i = 0; i < dynamicList.Count; ++i )
                {
                    ISqlNode a = dynamicList[i];
                    Debug.Assert( a != null );
                    var ve = VisitItem( a );
                    if( !ReferenceEquals( a, ve ) )
                    {
                        isModified = true;
                        if( ve == null ) dynamicList.RemoveAt( i-- );
                        else dynamicList[i] = ve;
                    }
                }
            }
            else
            {
                ISqlNode[] array = (ISqlNode[])nodes;
                for( int i = 0; i < array.Length; ++i )
                {
                    ISqlNode a = array[i];
                    var ve = a != null ? VisitItem( a ) : null;
                    if( !ReferenceEquals( a, ve ) )
                    {
                        isModified = true;
                        array[i] = ve;
                    }
                }
            }
            return isModified;
        }

        protected virtual ISqlNode VisitTokenStandard( SqlToken e ) => e;

        protected virtual ISqlNode VisitTypeDeclStandard( ISqlUnifiedTypeDecl e ) => e;

        internal protected virtual ISqlNode Visit( SqlTokenError e ) => e;

        #region All concrete SqlTokenXXX call VisitTokenStandard by default.

        internal protected virtual ISqlNode Visit( SqlTokenLiteralInteger e ) => VisitTokenStandard( e );

        internal protected virtual ISqlNode Visit( SqlTokenLiteralBinary e ) => VisitTokenStandard( e );

        internal protected virtual ISqlNode Visit( SqlTokenLiteralFloat e ) => VisitTokenStandard( e );

        internal protected virtual ISqlNode Visit( SqlTokenLiteralDecimal e ) => VisitTokenStandard( e );

        internal protected virtual ISqlNode Visit( SqlTokenLiteralMoney e ) => VisitTokenStandard( e );

        internal protected virtual ISqlNode Visit( SqlTokenLiteralString e ) => VisitTokenStandard( e );

        internal protected virtual ISqlNode Visit( SqlTokenTerminal e ) => VisitTokenStandard( e );

        internal protected virtual ISqlNode Visit( SqlTokenIdentifier e ) => VisitTokenStandard( e );

        #endregion


        #region All SqlTypeDeclXXX calls VisitTypeDeclStandard by default.

        internal protected virtual ISqlNode Visit( SqlTypeDeclDecimal e ) => VisitTypeDeclStandard( e );

        internal protected virtual ISqlNode Visit( SqlTypeDeclDateAndTime e ) => VisitTypeDeclStandard( e );

        internal protected virtual ISqlNode Visit( SqlTypeDeclSimple e ) => VisitTypeDeclStandard( e );

        internal protected virtual ISqlNode Visit( SqlTypeDeclWithSize e ) => VisitTypeDeclStandard( e );

        internal protected virtual ISqlNode Visit( SqlTypeDeclUserDefined e ) => VisitTypeDeclStandard( e );

        internal protected virtual ISqlNode Visit( SqlTypeDeclTable e ) => VisitTypeDeclStandard( e );

        internal protected virtual ISqlNode Visit( SqlTypeDeclCursorParameter e ) => VisitTypeDeclStandard( e );

        #endregion

        internal protected virtual ISqlNode Visit( SqlNodeExternal e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlGrant e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlOpenXml e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlOpenJSON e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCallParameter e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCallParameterList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlUnnamedStatement e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlNodeList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlEnclosableCommaList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlEnclosedCommaList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlPar e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlBasicValue e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlKoCall e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlOverClause e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCollate e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlIf e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCursorDefinition e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlIdentifierCommaList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCursorDefinition92 e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlDeclareCursor e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlBeginTransaction e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlStatementList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlBeginEndBlock e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTryCatch e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlStatement e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlEmptyStatement e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlReturnStatement e ) => VisitStandard( e );
        
        internal protected virtual ISqlNode Visit( SqlExecuteStatement e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlExecuteStringStatement e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlSetVariable e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlInsertStatement e ) => VisitStandard( e );
        
        internal protected virtual ISqlNode Visit( SqlUpdateStatement e ) => VisitStandard( e );
        
        internal protected virtual ISqlNode Visit( SqlDeleteStatement e ) => VisitStandard( e );
        
        internal protected virtual ISqlNode Visit( SqlMergeStatement e ) => VisitStandard( e );
        
        internal protected virtual ISqlNode Visit( SqlTableValues e ) => VisitStandard( e );
        
        internal protected virtual ISqlNode Visit( SqlMultiCommaList e ) => VisitStandard( e );
        
        internal protected virtual ISqlNode Visit( SqlWithParOptions e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlOptionParOptions e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlWithOptions e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( MIUDHeader e ) => VisitStandard( e );
        
        internal protected virtual ISqlNode Visit( IUDTarget e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlOutputClause e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlSetOption e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlGoto e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlWhile e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlRaiserror e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlLabelDefinition e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlView e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlFunctionScalar e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlFunctionInlineTable e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlFunctionTable e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlStoredProcedure e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTrigger e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlEnclosedIdentifierCommaList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlDeclareVariable e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlVariableDeclarationList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlVariableDeclaration e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCast e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCaseWhenList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlMultiIdentifier e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlOpenDataSource e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlUnaryOperator e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTypedIdentifier e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlParameter e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlParameterList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlAssign e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCommaList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlBinaryOperator e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlIsNull e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlLike e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlBetween e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlInValues e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCase e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCaseWhenSelector e ) => VisitStandard( e );

        #region Select

        internal protected virtual ISqlNode Visit( SqlSelectStatement e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectSpec e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectColumn e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectColumnList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectHeader e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectInto e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectFrom e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectGroupBy e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectCombine e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectDecorator e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectFor e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlOrderByList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlOrderByItem e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SelectOrderBy e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlNextValueFor e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCTEStatement e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCTENameList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlCTEName e ) => VisitStandard( e );

        #endregion

    }
}
