
namespace CK.SqlServer.Parser
{
    public partial class SqlNodeVisitor
    {
        /// <summary>
        /// Calls <see cref="VisitStandard(ISqlNode)"/>.
        /// </summary>
        /// <param name="e">The transformer node to visit.</param>
        /// <returns>The resulting node.</returns>
        internal protected virtual ISqlNode Visit( SqlTransformer e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTStatementList e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTAddParameter e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTAddColumn e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTInject e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTCombineSelect e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTInjectInto e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTReplace e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTInScope e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTOneLocationFinder e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTRangeLocationFinder e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTMultiLocationFinder e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTCurlyPattern e ) => VisitStandard( e );

        /// <inheritdoc cref="Visit(SqlTransformer)"/>
        internal protected virtual ISqlNode Visit( SqlTNodeSimplePattern e ) => VisitStandard( e );
        
    }
}
