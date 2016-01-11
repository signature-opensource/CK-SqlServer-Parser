using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public enum StatementKnownName
    {
        Empty,
        BeginTransaction,
        Goto,
        If,
        LabelDefinition,
        Return,
        TryCatch,
        BeginEnd,
        Select,
        CTE,
        Insert,
        Update,
        Merge,
        Delete,
        Execute,
        ExecuteString,
        CreateView,
        AlterView,
        CreateFunction,
        AlterFunction,
        CreateProcedure,
        AlterProcedure,
        SetVariable,
        SetOption,
        DeclareVariable,
        DeclareCursor,
        /// <summary>
        /// General statement name.
        /// </summary>
        Unknown
    }
}
