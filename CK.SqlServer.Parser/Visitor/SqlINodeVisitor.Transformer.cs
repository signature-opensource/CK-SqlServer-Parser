using System;
using System.Collections.Generic;
using System.Linq;
using CK.Core;
using System.Diagnostics;

namespace CK.SqlServer.Parser
{
    public partial class SqlNodeVisitor
    {
        internal protected virtual ISqlNode Visit( SqlTransformer e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTStatementList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTAddParameter e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTInject e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTLocationFinder e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTCurlyPattern e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTNodeSimplePattern e ) => VisitStandard( e );

        
    }
}
