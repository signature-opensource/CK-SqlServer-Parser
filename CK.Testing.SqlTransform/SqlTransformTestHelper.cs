using CK.SqlServer.Parser;
using CK.Testing.SqlTransform;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CK.Testing
{
    /// <summary>
    /// Standard implementation of <see cref="ISqlTransformHelperCore"/>.
    /// </summary>
    public class SqlTransformTestHelper : ISqlTransformHelperCore
    {
        SqlServerParser _parser;

        SqlServerParser ISqlTransformHelperCore.SqlServerParser => _parser ?? (_parser = new SqlServerParser());

        string ISqlTransformHelperCore.GetObjectDefinition( string connectionString, string schemaName )
        {
            using( var oCon = new SqlConnection( connectionString ) )
            {
                oCon.Open();
                return DoGetObjectDefinition( oCon, schemaName );
            }
        }

        IDisposable ISqlTransformHelperCore.TemporaryTransform( string connectionString, string transformer ) => DoTemporaryTransform( connectionString, transformer );

        string DoGetObjectDefinition( SqlConnection oCon, string schemaName )
        {
            using( var cmd = new SqlCommand( "select OBJECT_DEFINITION(OBJECT_ID(@0))" ) { Connection = oCon } )
            {
                cmd.Parameters.AddWithValue( "@0", schemaName );
                return (string)cmd.ExecuteScalar();
            }
        }

        class Restorer : IDisposable
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

        IDisposable DoTemporaryTransform( string connectionString, string transformer )
        {
            var tResult = TestHelper.SqlServerParser.ParseTransformer( transformer );
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
                var oResult = TestHelper.SqlServerParser.ParseObject( origin );
                if( oResult.IsError )
                {
                    throw new Exception( "Unable to parse object definition: " + oResult.ErrorMessage );
                }
                ISqlServerObject o = oResult.Result;
                ISqlServerObject oT = t.SafeTransform( TestHelper.Monitor, o );
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
                IDisposable restorer = new Restorer( oCon.ConnectionString, origin, oType, o.SchemaName );
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
        
        static void ExecuteNonQuery( SqlConnection oCon, string c )
        {
            using( var cmd = new SqlCommand( c ) { Connection = oCon } )
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the <see cref="ISqlTransformHelper"/> default implementation.
        /// </summary>
        public static ISqlTransformHelper TestHelper => TestHelperResolver.Default.Resolve<ISqlTransformHelper>();

    }
}
