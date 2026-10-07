using CK.SqlServer.Parser;
using System;
using Microsoft.Data.SqlClient;

namespace CK.Testing;

/// <summary>
/// Extends <see cref="IMonitorTestHelper"/>.
/// </summary>
public static class SqlTransformTestExtensions
{
    static SqlServerParser? _parser;

    /// <summary>
    /// Gets a shared reusable <see cref="SqlServerParser"/>.
    /// This not (no more) exposed publicly because the SqlServerParser should be superseded with a parser from
    /// new Transform layer...
    /// </summary>
    static SqlServerParser SqlServerParser => _parser ?? (_parser = new SqlServerParser());

    /// <summary>
    /// Returns the object text definition of <paramref name="schemaName"/> object.
    /// </summary>
    /// <param name="helper">This test helper.</param>
    /// <param name="connectionString">Connection string to the database.</param>
    /// <param name="schemaName">Name of the object.</param>
    /// <returns>The text.</returns>
    public static string GetObjectDefinition( this IMonitorTestHelper helper, string connectionString, string schemaName )
    {
        using( var oCon = new SqlConnection( connectionString ) )
        {
            oCon.Open();
            return DoGetObjectDefinition( oCon, schemaName );
        }
    }

    static string DoGetObjectDefinition( SqlConnection oCon, string schemaName )
    {
        using( var cmd = new SqlCommand( "select OBJECT_DEFINITION(OBJECT_ID(@0))" ) { Connection = oCon } )
        {
            cmd.Parameters.AddWithValue( "@0", schemaName );
            return (string)cmd.ExecuteScalar();
        }
    }

    /// <summary>
    /// Applies a temporary transformation. The transformer must target an existing
    /// sql object that will be restored when the returned IDisposable.Dispose() method is called. 
    /// </summary>
    /// <param name="helper">This test helper.</param>
    /// <param name="connectionString">Connection string to the database.</param>
    /// <param name="transformer">Transformer text.</param>
    /// <returns>A disposable object that will restore the original object.</returns>
    public static IDisposable TemporaryTransform( this IMonitorTestHelper helper, string connectionString, string transformer ) 
    {
        var tResult = SqlServerParser.ParseTransformer( transformer );
        if( tResult.IsError )
        {
            throw new ArgumentException( "Invalid transformation: " + tResult.ErrorMessage, nameof( transformer ) );
        }
        ISqlServerTransformer t = tResult.Result;
        string targetName = t.TargetSchemaName;
        if( targetName == null )
        {
            throw new ArgumentException( "Transfomer must target a Sql object.", nameof( transformer ) );
        }
        using( var oCon = new SqlConnection( connectionString ) )
        {
            oCon.Open();

            string origin = DoGetObjectDefinition( oCon, targetName );
            var oResult = SqlServerParser.ParseObject( origin );
            if( oResult.IsError )
            {
                throw new Exception( "Unable to parse object definition: " + oResult.ErrorMessage );
            }
            ISqlServerObject o = oResult.Result;
            ISqlServerObject oT = t.SafeTransform( helper.Monitor, o );
            if( oT == null )
            {
                throw new Exception( "Unable to apply transformer." );
            }
            string oType;
            switch( o.ObjectType )
            {
                case SqlServerObjectType.Procedure: oType = "procedure"; break;
                case SqlServerObjectType.View: oType = "view"; break;
                default: oType = "function"; break;
            }
            IDisposable restorer = new Restorer( connectionString, origin, oType, o.SchemaName );
            try
            {
                ExecuteNonQuery( oCon, $"drop {oType} {o.SchemaName};" );
                ExecuteNonQuery( oCon, oT.ToFullString() );
                return restorer;
            }
            catch
            {
                restorer.Dispose();
                throw;
            }
        }
    }


    sealed class Restorer : IDisposable
    {
        readonly string _connectionString;
        readonly string _original;
        readonly string _oType;
        readonly string _schemaName;

        public Restorer( string c, string original, string type, string schemaName )
        {
            _connectionString = c;
            _original = original;
            _oType = type;
            _schemaName = schemaName;
        }

        void IDisposable.Dispose()
        {
            var safe = _schemaName.Replace( "'", "''" );
            using( var oCon = new SqlConnection( _connectionString ) )
            {
                oCon.Open();
                ExecuteNonQuery( oCon, $"if OBJECT_ID('{safe}') is not null drop {_oType} {_schemaName};" );
                ExecuteNonQuery( oCon, _original );
            }
        }
    }

    static void ExecuteNonQuery( SqlConnection oCon, string c )
    {
        using( var cmd = new SqlCommand( c ) { Connection = oCon } )
        {
            cmd.ExecuteNonQuery();
        }
    }

}
