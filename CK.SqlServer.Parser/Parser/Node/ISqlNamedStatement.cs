using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser
{
    public interface ISqlNamedStatement : ISqlStatement
    {
        StatementName StatementName { get; }
    }

    public enum StatementName
    {
        None,
        BeginTransaction,
        Goto,
        If,
        LabelDefinition,
        Return,
        TryCatch,
        BeginEnd,
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
        Statement
    }

}
