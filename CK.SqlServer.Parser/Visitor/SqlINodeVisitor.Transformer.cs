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

        internal protected virtual ISqlNode Visit( SqlTransformStatementList e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTAddParameter e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTInsert e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTLocationSelector e ) => VisitStandard( e );
    }
}
