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
        BeginEndBlock,
        While,
        Select,
        CTE,
        Insert,
        Update,
        Merge,
        Delete,
        Grant,
        Raiserror,
        Execute,
        ExecuteString,
        CreateView,
        AlterView,
        CreateFunction,
        AlterFunction,
        CreateProcedure,
        AlterProcedure,
        CreateTrigger,
        AlterTrigger,
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
