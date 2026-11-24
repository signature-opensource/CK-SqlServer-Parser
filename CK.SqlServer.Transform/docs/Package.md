The T-SQL transformers: applies the transformers written in the TQL language to the immutable nodes parsed by
CK.SqlServer.Parser.

A transformer is a `create transformer on <target> as begin ... end` statement. `SqlTransformHost.Transform` applies a
`SqlTransformer` to a target `ISqlNode`; `SqlTransformExtension` offers helpers on the parsed objects.
