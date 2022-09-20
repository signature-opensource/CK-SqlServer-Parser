using CK.SqlServer.Parser;
using System;

namespace CK.Testing.SqlTransform
{
    /// <summary>
    /// Supports temporary transformation of sql server objects.
    /// </summary>
    public interface ISqlTransformTestHelperCore
    {
        /// <summary>
        /// Gets a shared reusable <see cref="SqlServerParser"/>.
        /// </summary>
        SqlServerParser SqlServerParser { get; }

        /// <summary>
        /// Returns the object text definition of <paramref name="schemaName"/> object.
        /// </summary>
        /// <param name="connectionString">Connection string to the database.</param>
        /// <param name="schemaName">Name of the object.</param>
        /// <returns>The text.</returns>
        string GetObjectDefinition( string connectionString, string schemaName );

        /// <summary>
        /// Applies a temporary transformation. The transformer must target an existing
        /// sql object that will be restored when the returned IDisposable.Dispose() method is called. 
        /// </summary>
        /// <param name="connectionString">Connection string to the database.</param>
        /// <param name="transformer">Transformer text.</param>
        /// <returns>A disposable object that will restore the original object.</returns>
        IDisposable TemporaryTransform( string connectionString, string transformer );
    }
}
