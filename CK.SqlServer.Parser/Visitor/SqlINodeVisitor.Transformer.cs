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

        internal protected virtual ISqlNode Visit( SqlTAddColumn e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTInject e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTCombineSelect e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTInjectInto e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTReplace e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTInScope e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTOneLocationFinder e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTRangeLocationFinder e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTMultiLocationFinder e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTCurlyPattern e ) => VisitStandard( e );

        internal protected virtual ISqlNode Visit( SqlTNodeSimplePattern e ) => VisitStandard( e );

        
    }
}
