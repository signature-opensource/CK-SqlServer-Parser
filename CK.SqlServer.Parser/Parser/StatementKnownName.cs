#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser
{

    /// <summary>
    /// Defines the statement that are currently modeled.
    /// </summary>
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
        CreateOrAlterView,
        CreateFunction,
        AlterFunction,
        CreateOrAlterFunction,
        CreateProcedure,
        AlterProcedure,
        CreateOrAlterProcedure,
        CreateTrigger,
        AlterTrigger,
        CreateOrAlterTrigger,
        SetVariable,
        SetOption,
        DeclareVariable,
        DeclareCursor,
        WithForXml,
        /// <summary>
        /// General statement name.
        /// </summary>
        Unknown,
        // This is not Sql :)
        CreateTransformer,
    }
}
