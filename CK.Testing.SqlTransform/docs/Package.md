Adds SQL transformation helpers to the CK.Testing test helper: a temporary transformation of a database object, restored
when it is disposed.

`SqlTransformTestExtensions.TemporaryTransform` applies a transformer to an existing object of a database and
`SqlTransformTestExtensions.GetObjectDefinition` reads the text of an object.
