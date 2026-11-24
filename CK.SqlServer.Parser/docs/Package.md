The T-SQL parser: implements the `ISqlServerParser` of CK.SqlServer.Parser.Model and produces an immutable tree of
nodes.

`SqlServerParser` parses objects (`SqlServerParser.ParseObject`, `SqlServerParser.ParseStoredProcedure`,
`SqlServerParser.ParseView`...), transformers (`SqlServerParser.ParseTransformer`) and scripts
(`SqlServerParser.ParseScript`); the nodes are `ISqlNode`.
