using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.SqlServer.Parser
{
    public static class SqlKeyword
    {
        #region Arrays of keywords

        /// <summary>
        /// Mapped to SqlTokenType.IdentifierReserved.
        /// </summary>
        static string[] _sqlServerReserved = new string[] 
        {
            "freetext",
            "freetexttable",
            "reconfigure",
            "references",
            "full",
            "replication",
            "bulk",
            "check",
            "holdlock",
            "right",
            "checkpoint",
            "identity",
            "identity_insert",
            "rowcount",
            "save",
            "column",
            "index",
            "schema",
            "inner",
            "securityaudit",
            "compute",
            "constraint",
            "session_user",
            "setuser",
            "join",
            "shutdown",
            "convert",
            "key",
            "statistics",
            "system_user",
            "current",
            "current_date",
            "lineno",
            "tablesample",
            "current_time",
            "load",
            "textsize",
            "current_timestamp",
            "current_user",
            "national",
            "nullif",
            "tsequal",
            "off",
            "offsets",
            "unique",
            "unpivot",
            "disk",
            "dump",
            "varying",
            "errlvl",

            "restrict",
            "cascade",
            "revert",
            "revoke",
            "any",
            "some",
            "precision",
            "exit",
            "primary",
            "plan",
            "file",
            "fillfactor",
            "public",
            "authorization",
            "distributed",
            "coalesce",
            "rule",
            "identitycol",
            "rowguidcol",
            "contains",
            "containstable",
            "nocheck",
            "nonclustered",
            "double",
            "outer",

            // These keywords are explicitly associated to a SqlTokenType (OpLevelXX | IdentifierReserved | YY).
            // "or",
            // "and",
            // "not",
            // "between",
            // "in",
            // "is",
            // "like",
            // "union",
            // "intersect",
            // "except",
            // "order",
            // "for",
            // "over",

            // These keywords are explicitly associated to a SqlTokenType (IdentifierStandard | YY).
            // "case",
            // "when",
            // "null",
            // "when",
            // "by",
            // "all",
            // "then",
            // "else",
            // "tran", "transaction",   // Both map to SqlTokenType.Transaction.
            // "with",                  // Considered as a normal reserved keyword (not a IdentifierReservedStart) since it is mandatory to put a ; before it.
            // "proc", "procedure",
            // "function",
            // "view",
            // "table",
            // "database",
            // "trigger",
            // "as",
            // "asc",
            // "desc",
            // "exists",
            // "on",
            // "to",
            // "of",
            // "top",
            // "escape",
            // "into", 
            // "from", 
            // "where",
            // "group",
            // "option",
            // "add",
            // "max",
            // "output",
            // "readonly",
            // "cross",
            // "foreign",
            // "clustered",
            // "left",
            // "percent",
            // "values",
            // "distinct",
            // "cursor",
            // "scroll",
            // "insensitive",
            // "read",
            // "pivot",


        };

        #endregion

        static Dictionary<string,SqlTokenType> _keywords;

        static SqlDbType[] _sqlDbTypesMapped = new SqlDbType[]
            {
                SqlDbType.Xml,
                SqlDbType.DateTimeOffset,
                SqlDbType.DateTime2,
                SqlDbType.DateTime,
                SqlDbType.SmallDateTime,
                SqlDbType.Date,
                SqlDbType.Time,
                SqlDbType.Float,
                SqlDbType.Real,
                SqlDbType.Decimal,
                SqlDbType.Money,
                SqlDbType.SmallMoney,
                SqlDbType.BigInt,
                SqlDbType.Int,
                SqlDbType.SmallInt,
                SqlDbType.TinyInt,
                SqlDbType.Bit,
                SqlDbType.NText,
                SqlDbType.Text,
                SqlDbType.Image,
                SqlDbType.Timestamp,
                SqlDbType.UniqueIdentifier,
                SqlDbType.NVarChar,
                SqlDbType.NChar,
                SqlDbType.VarChar,
                SqlDbType.Char,
                SqlDbType.VarBinary,
                SqlDbType.Binary,
                SqlDbType.Variant,
                SqlDbType.Structured,
            };

        static SqlKeyword()
        {
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.XmlDbType                & SqlTokenType.IdentifierValueMask)] == SqlDbType.Xml );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.DateTimeOffsetDbType     & SqlTokenType.IdentifierValueMask)] == SqlDbType.DateTimeOffset );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.DateTime2DbType          & SqlTokenType.IdentifierValueMask)] == SqlDbType.DateTime2 );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.DateTimeDbType           & SqlTokenType.IdentifierValueMask)] == SqlDbType.DateTime );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.SmallDateTimeDbType      & SqlTokenType.IdentifierValueMask)] == SqlDbType.SmallDateTime );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.DateDbType               & SqlTokenType.IdentifierValueMask)] == SqlDbType.Date );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.TimeDbType               & SqlTokenType.IdentifierValueMask)] == SqlDbType.Time );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.FloatDbType              & SqlTokenType.IdentifierValueMask)] == SqlDbType.Float );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.RealDbType               & SqlTokenType.IdentifierValueMask)] == SqlDbType.Real );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.DecimalDbType            & SqlTokenType.IdentifierValueMask)] == SqlDbType.Decimal );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.MoneyDbType              & SqlTokenType.IdentifierValueMask)] == SqlDbType.Money );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.SmallMoneyDbType         & SqlTokenType.IdentifierValueMask)] == SqlDbType.SmallMoney );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.BigIntDbType             & SqlTokenType.IdentifierValueMask)] == SqlDbType.BigInt );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.IntDbType                & SqlTokenType.IdentifierValueMask)] == SqlDbType.Int );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.SmallIntDbType           & SqlTokenType.IdentifierValueMask)] == SqlDbType.SmallInt );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.TinyIntDbType            & SqlTokenType.IdentifierValueMask)] == SqlDbType.TinyInt );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.BitDbType                & SqlTokenType.IdentifierValueMask)] == SqlDbType.Bit );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.NTextDbType              & SqlTokenType.IdentifierValueMask)] == SqlDbType.NText );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.TextDbType               & SqlTokenType.IdentifierValueMask)] == SqlDbType.Text );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.ImageDbType              & SqlTokenType.IdentifierValueMask)] == SqlDbType.Image );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.TimestampDbType          & SqlTokenType.IdentifierValueMask)] == SqlDbType.Timestamp );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.UniqueIdentifierDbType   & SqlTokenType.IdentifierValueMask)] == SqlDbType.UniqueIdentifier );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.NVarCharDbType           & SqlTokenType.IdentifierValueMask)] == SqlDbType.NVarChar );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.NCharDbType              & SqlTokenType.IdentifierValueMask)] == SqlDbType.NChar );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.VarCharDbType            & SqlTokenType.IdentifierValueMask)] == SqlDbType.VarChar );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.CharDbType               & SqlTokenType.IdentifierValueMask)] == SqlDbType.Char );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.VarBinaryDbType          & SqlTokenType.IdentifierValueMask)] == SqlDbType.VarBinary );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.BinaryDbType             & SqlTokenType.IdentifierValueMask)] == SqlDbType.Binary );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.VariantDbType            & SqlTokenType.IdentifierValueMask)] == SqlDbType.Variant );
            Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.TableDbType              & SqlTokenType.IdentifierValueMask)] == SqlDbType.Structured );

            _keywords = new Dictionary<string, SqlTokenType>( StringComparer.InvariantCultureIgnoreCase );

            // Identifiers mapped to SqlTokenType.
            
            // SqlDbType mapping.
            _keywords.Add( "sql_variant", SqlTokenType.VariantDbType );
            _keywords.Add( "xml", SqlTokenType.XmlDbType );
            _keywords.Add( "datetimeoffset", SqlTokenType.DateTimeOffsetDbType );
            _keywords.Add( "datetime2", SqlTokenType.DateTime2DbType );
            _keywords.Add( "datetime", SqlTokenType.DateTimeDbType );
            _keywords.Add( "smalldatetime", SqlTokenType.SmallDateTimeDbType );
            _keywords.Add( "date", SqlTokenType.DateDbType );
            _keywords.Add( "time", SqlTokenType.TimeDbType );
            _keywords.Add( "float", SqlTokenType.FloatDbType );
            _keywords.Add( "real", SqlTokenType.RealDbType );
            _keywords.Add( "decimal", SqlTokenType.DecimalDbType );
            _keywords.Add( "numeric", SqlTokenType.DecimalDbType );
            _keywords.Add( "money", SqlTokenType.MoneyDbType );
            _keywords.Add( "smallmoney", SqlTokenType.SmallMoneyDbType );
            _keywords.Add( "bigint", SqlTokenType.BigIntDbType );
            _keywords.Add( "int", SqlTokenType.IntDbType );
            _keywords.Add( "smallint", SqlTokenType.SmallIntDbType );
            _keywords.Add( "tinyint", SqlTokenType.TinyIntDbType );
            _keywords.Add( "bit", SqlTokenType.BitDbType );
            _keywords.Add( "ntext", SqlTokenType.NTextDbType );
            _keywords.Add( "text", SqlTokenType.TextDbType );
            _keywords.Add( "image", SqlTokenType.ImageDbType );
            _keywords.Add( "timestamp", SqlTokenType.TimestampDbType );
            _keywords.Add( "uniqueidentifier", SqlTokenType.UniqueIdentifierDbType );
            _keywords.Add( "nvarchar", SqlTokenType.NVarCharDbType );
            _keywords.Add( "nchar", SqlTokenType.NCharDbType );
            _keywords.Add( "varchar", SqlTokenType.VarCharDbType );
            _keywords.Add( "char", SqlTokenType.CharDbType );
            _keywords.Add( "varbinary", SqlTokenType.VarBinaryDbType );
            _keywords.Add( "binary", SqlTokenType.BinaryDbType );
            _keywords.Add( "table", SqlTokenType.TableDbType );

            Debug.Assert( _keywords.Values.All( t => !t.IsReservedKeyword()
                                                    || t == SqlTokenType.TableDbType ), 
                                            "Sql database type names are not reserved keyworkds except 'table'." );

            // SqlTokenType.IdentifierStandardStatement values: these are not reserved keywords but they can start a statement.
            _keywords.Add( "throw", SqlTokenType.Throw );
            _keywords.Add( "get", SqlTokenType.Get );
            _keywords.Add( "move", SqlTokenType.Move );
            _keywords.Add( "receive", SqlTokenType.Receive );
            _keywords.Add( "send", SqlTokenType.Send );

            // SqlTokenType.IdentifierStandard values: these are not reserved keywords.
            _keywords.Add( "try", SqlTokenType.Try );
            _keywords.Add( "catch", SqlTokenType.Catch );
            _keywords.Add( "dialog", SqlTokenType.Dialog );
            _keywords.Add( "conversation", SqlTokenType.Conversation );
            _keywords.Add( "returns", SqlTokenType.Returns );
            _keywords.Add( "max", SqlTokenType.Max );
            _keywords.Add( "insensitive", SqlTokenType.Insensitive );
            _keywords.Add( "scroll", SqlTokenType.Scroll );
            _keywords.Add( "mark", SqlTokenType.Mark );
            _keywords.Add( "json", SqlTokenType.Json );
            _keywords.Add( "system_time", SqlTokenType.SystemTime );
            _keywords.Add( "ties", SqlTokenType.Ties );
            _keywords.Add( "readonly", SqlTokenType.Readonly );
            _keywords.Add( "out", SqlTokenType.Output );
            _keywords.Add( "output", SqlTokenType.Output );
            _keywords.Add( "row", SqlTokenType.Rows );
            _keywords.Add( "rows", SqlTokenType.Rows );
            _keywords.Add( "offset", SqlTokenType.Offset );
            _keywords.Add( "first", SqlTokenType.First );
            _keywords.Add( "next", SqlTokenType.Next );
            _keywords.Add( "only", SqlTokenType.Only );
            _keywords.Add( "cast", SqlTokenType.Cast );
            _keywords.Add( "value", SqlTokenType.Value );
            _keywords.Add( "matched", SqlTokenType.Matched );
            _keywords.Add( "recompile", SqlTokenType.Recompile );
            _keywords.Add( "result", SqlTokenType.Result );
            _keywords.Add( "sets", SqlTokenType.Sets );
            _keywords.Add( "undefined", SqlTokenType.Undefined );
            _keywords.Add( "login", SqlTokenType.Login );
            _keywords.Add( "at", SqlTokenType.At );

            // LogicalOperator (they are reserved keywords).
            _keywords.Add( "or", SqlTokenType.Or );
            _keywords.Add( "and", SqlTokenType.And );
            _keywords.Add( "not", SqlTokenType.Not );
            // CompareOperator (they are reserved keywords).
            _keywords.Add( "between", SqlTokenType.Between );
            _keywords.Add( "in", SqlTokenType.In );
            _keywords.Add( "is", SqlTokenType.Is );
            _keywords.Add( "like", SqlTokenType.Like );
            // Select operators (they are reserved keywords).
            _keywords.Add( "union", SqlTokenType.Union );
            _keywords.Add( "intersect", SqlTokenType.Intersect );
            _keywords.Add( "except", SqlTokenType.Except );
            _keywords.Add( "order", SqlTokenType.Order );
            _keywords.Add( "for", SqlTokenType.For );

            // SqlTokenType.IdentifierReserved values.
            _keywords.Add( "case", SqlTokenType.Case );
            _keywords.Add( "null", SqlTokenType.Null );
            _keywords.Add( "when", SqlTokenType.When );
            _keywords.Add( "by", SqlTokenType.By );
            _keywords.Add( "all", SqlTokenType.All );
            _keywords.Add( "then", SqlTokenType.Then );
            _keywords.Add( "else", SqlTokenType.Else );
            _keywords.Add( "tran", SqlTokenType.Transaction );
            _keywords.Add( "transaction", SqlTokenType.Transaction );
            _keywords.Add( "with", SqlTokenType.With );
            _keywords.Add( "proc", SqlTokenType.Procedure );
            _keywords.Add( "procedure", SqlTokenType.Procedure );
            _keywords.Add( "function", SqlTokenType.Function );
            _keywords.Add( "view", SqlTokenType.View );
            _keywords.Add( "trigger", SqlTokenType.Trigger );
            _keywords.Add( "as", SqlTokenType.As );
            _keywords.Add( "asc", SqlTokenType.Asc );
            _keywords.Add( "desc", SqlTokenType.Desc );
            _keywords.Add( "exists", SqlTokenType.Exists );
            _keywords.Add( "on", SqlTokenType.On );
            _keywords.Add( "to", SqlTokenType.To );
            _keywords.Add( "of", SqlTokenType.Of );
            _keywords.Add( "top", SqlTokenType.Top );
            _keywords.Add( "escape", SqlTokenType.Escape );
            _keywords.Add( "into",  SqlTokenType.Into );
            _keywords.Add( "from",  SqlTokenType.From );
            _keywords.Add( "where", SqlTokenType.Where );
            _keywords.Add( "group", SqlTokenType.Group );
            _keywords.Add( "option", SqlTokenType.Option );
            _keywords.Add( "add", SqlTokenType.Add );
            _keywords.Add( "database", SqlTokenType.Database );
            _keywords.Add( "external", SqlTokenType.External );
            _keywords.Add( "over", SqlTokenType.Over );
            _keywords.Add( "cross", SqlTokenType.Cross );
            _keywords.Add( "foreign", SqlTokenType.Foreign );
            _keywords.Add( "clustered", SqlTokenType.Clustered );
            _keywords.Add( "left", SqlTokenType.Left );
            _keywords.Add( "percent", SqlTokenType.Percent );
            _keywords.Add( "values", SqlTokenType.Values );
            _keywords.Add( "distinct", SqlTokenType.Distinct );
            _keywords.Add( "pivot", SqlTokenType.Pivot );
            _keywords.Add( "having", SqlTokenType.Having );
            _keywords.Add( "cursor", SqlTokenType.Cursor );
            _keywords.Add( "read", SqlTokenType.Read );
            _keywords.Add( "browse", SqlTokenType.Browse );
            _keywords.Add( "collate", SqlTokenType.Collate );
            _keywords.Add( "opendatasource", SqlTokenType.OpenDataSource );
            _keywords.Add( "openrowset", SqlTokenType.OpenRowSet );
            _keywords.Add( "openxml", SqlTokenType.OpenXml );
            _keywords.Add( "openquery", SqlTokenType.OpenQuery );
            _keywords.Add( "default", SqlTokenType.Default );
            _keywords.Add( "user", SqlTokenType.User );

            // SqlTokenType.IdentifierReservedStatement values.
            _keywords.Add( "select", SqlTokenType.Select );
            _keywords.Add( "begin", SqlTokenType.Begin );
            _keywords.Add( "end", SqlTokenType.End );
            _keywords.Add( "create", SqlTokenType.Create );
            _keywords.Add( "drop", SqlTokenType.Drop );
            _keywords.Add( "alter", SqlTokenType.Alter );
            _keywords.Add( "declare", SqlTokenType.Declare );
            _keywords.Add( "break", SqlTokenType.Break );
            _keywords.Add( "continue", SqlTokenType.Continue );
            _keywords.Add( "goto", SqlTokenType.Goto );
            _keywords.Add( "while", SqlTokenType.While );
            _keywords.Add( "if", SqlTokenType.If );
            _keywords.Add( "deallocate", SqlTokenType.Deallocate );
            _keywords.Add( "close", SqlTokenType.Close );
            _keywords.Add( "fetch", SqlTokenType.Fetch );
            _keywords.Add( "open", SqlTokenType.Open );
            _keywords.Add( "return", SqlTokenType.Return );
            _keywords.Add( "exec", SqlTokenType.Execute );
            _keywords.Add( "execute", SqlTokenType.Execute );
            _keywords.Add( "set", SqlTokenType.Set );
            _keywords.Add( "update", SqlTokenType.Update );
            _keywords.Add( "insert", SqlTokenType.Insert );
            _keywords.Add( "raiserror", SqlTokenType.Raiserror );
            _keywords.Add( "waitfor", SqlTokenType.WaitFor );
            _keywords.Add( "use", SqlTokenType.Use );
            _keywords.Add( "truncate", SqlTokenType.Truncate );
            _keywords.Add( "print", SqlTokenType.Print );
            _keywords.Add( "commit", SqlTokenType.Commit );
            _keywords.Add( "rollback", SqlTokenType.Rollback );
            _keywords.Add( "delete", SqlTokenType.Delete );
            _keywords.Add( "updatetext", SqlTokenType.Updatetext );
            _keywords.Add( "merge", SqlTokenType.Merge );
            _keywords.Add( "kill", SqlTokenType.Kill );
            _keywords.Add( "readtext", SqlTokenType.Readtext );
            _keywords.Add( "writetext", SqlTokenType.Writetext );
            _keywords.Add( "dbcc", SqlTokenType.Dbcc );
            _keywords.Add( "go", SqlTokenType.Go );
            _keywords.Add( "backup", SqlTokenType.Backup );
            _keywords.Add( "restore", SqlTokenType.Restore );
            _keywords.Add( "grant", SqlTokenType.Grant );
            _keywords.Add( "deny", SqlTokenType.Deny );

            // Reserved keywords.
            foreach( string s in _sqlServerReserved )
            {
                #if DEBUG
                if( _keywords.ContainsKey( s ) ) Debugger.Break();
                #endif
                _keywords.Add( s, SqlTokenType.IdentifierReserved );
            }
        }

        public static SqlDbType? FromSqlTokenTypeToSqlDbType( SqlTokenType t )
        {
            if( t < 0 
                || (t & SqlTokenType.IsIdentifier) == 0 
                || ((t & SqlTokenType.IdentifierTypeMask) != SqlTokenType.IdentifierDbType
                    && (t & SqlTokenType.IdentifierTypeMask) != SqlTokenType.IdentifierReservedDbType) ) return null;
            int iT = (int)(t & SqlTokenType.IdentifierValueMask);
            return _sqlDbTypesMapped[iT];
        }

        public static bool IsReservedKeyword( string s )
        {
            SqlTokenType tokenType;
            return IsReservedKeyword( s, out tokenType );
        }

        public static bool IsReservedKeyword( string s, out SqlTokenType tokenType )
        {
            return _keywords.TryGetValue( s, out tokenType ) && tokenType.IsReservedKeyword();
        }

        public static SqlTokenType MapKeyword( string s )
        {
            SqlTokenType mapped;
            _keywords.TryGetValue( s, out mapped );
            return mapped;
        }
    }
}
