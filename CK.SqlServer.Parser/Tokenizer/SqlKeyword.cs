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
        static Dictionary<string, SqlTokenType> _keywords;
        static Dictionary<SqlTokenType, string> _typeToString;

        static void RegKeyword( string name, SqlTokenType t )
        {
            _keywords.Add( name, t );
            if( _typeToString.ContainsKey( t ) ) Debugger.Break();
            _typeToString.Add( t, name );
        }

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
            _typeToString = new Dictionary<SqlTokenType, string>();

            // Identifiers mapped to SqlTokenType.

            // SqlDbType mapping.
            RegKeyword( "sql_variant", SqlTokenType.VariantDbType );
            RegKeyword( "xml", SqlTokenType.XmlDbType );
            RegKeyword( "datetimeoffset", SqlTokenType.DateTimeOffsetDbType );
            RegKeyword( "datetime2", SqlTokenType.DateTime2DbType );
            RegKeyword( "datetime", SqlTokenType.DateTimeDbType );
            RegKeyword( "smalldatetime", SqlTokenType.SmallDateTimeDbType );
            RegKeyword( "date", SqlTokenType.DateDbType );
            RegKeyword( "time", SqlTokenType.TimeDbType );
            RegKeyword( "float", SqlTokenType.FloatDbType );
            RegKeyword( "real", SqlTokenType.RealDbType );
            RegKeyword( "decimal", SqlTokenType.DecimalDbType );
            _keywords.Add( "numeric", SqlTokenType.DecimalDbType );
            RegKeyword( "money", SqlTokenType.MoneyDbType );
            RegKeyword( "smallmoney", SqlTokenType.SmallMoneyDbType );
            RegKeyword( "bigint", SqlTokenType.BigIntDbType );
            RegKeyword( "int", SqlTokenType.IntDbType );
            RegKeyword( "smallint", SqlTokenType.SmallIntDbType );
            RegKeyword( "tinyint", SqlTokenType.TinyIntDbType );
            RegKeyword( "bit", SqlTokenType.BitDbType );
            RegKeyword( "ntext", SqlTokenType.NTextDbType );
            RegKeyword( "text", SqlTokenType.TextDbType );
            RegKeyword( "image", SqlTokenType.ImageDbType );
            RegKeyword( "timestamp", SqlTokenType.TimestampDbType );
            RegKeyword( "uniqueidentifier", SqlTokenType.UniqueIdentifierDbType );
            RegKeyword( "nvarchar", SqlTokenType.NVarCharDbType );
            RegKeyword( "nchar", SqlTokenType.NCharDbType );
            RegKeyword( "varchar", SqlTokenType.VarCharDbType );
            RegKeyword( "char", SqlTokenType.CharDbType );
            RegKeyword( "varbinary", SqlTokenType.VarBinaryDbType );
            RegKeyword( "binary", SqlTokenType.BinaryDbType );
            RegKeyword( "table", SqlTokenType.TableDbType );

            Debug.Assert( _keywords.Values.All( t => !t.IsReservedKeyword()
                                                    || t == SqlTokenType.TableDbType ), 
                                            "Sql database type names are not reserved keyworkds except 'table'." );

            // SqlTokenType.IdentifierStandardStatement values: these are not reserved keywords but they can start a statement.
            RegKeyword( "throw", SqlTokenType.Throw );
            RegKeyword( "get", SqlTokenType.Get );
            RegKeyword( "move", SqlTokenType.Move );
            RegKeyword( "receive", SqlTokenType.Receive );
            RegKeyword( "send", SqlTokenType.Send );

            // SqlTokenType.IdentifierStandard values: these are not reserved keywords.
            RegKeyword( "try", SqlTokenType.Try );
            RegKeyword( "catch", SqlTokenType.Catch );
            RegKeyword( "dialog", SqlTokenType.Dialog );
            RegKeyword( "conversation", SqlTokenType.Conversation );
            RegKeyword( "returns", SqlTokenType.Returns );
            RegKeyword( "max", SqlTokenType.Max );
            RegKeyword( "insensitive", SqlTokenType.Insensitive );
            RegKeyword( "scroll", SqlTokenType.Scroll );
            RegKeyword( "mark", SqlTokenType.Mark );
            RegKeyword( "json", SqlTokenType.Json );
            RegKeyword( "system_time", SqlTokenType.SystemTime );
            RegKeyword( "ties", SqlTokenType.Ties );
            RegKeyword( "readonly", SqlTokenType.Readonly );
            RegKeyword( "output", SqlTokenType.Output );
            _keywords.Add( "out", SqlTokenType.Output );
            RegKeyword( "rows", SqlTokenType.Rows );
            _keywords.Add( "row", SqlTokenType.Rows );
            RegKeyword( "offset", SqlTokenType.Offset );
            RegKeyword( "first", SqlTokenType.First );
            RegKeyword( "next", SqlTokenType.Next );
            RegKeyword( "only", SqlTokenType.Only );
            RegKeyword( "cast", SqlTokenType.Cast );
            RegKeyword( "value", SqlTokenType.Value );
            RegKeyword( "matched", SqlTokenType.Matched );
            RegKeyword( "recompile", SqlTokenType.Recompile );
            RegKeyword( "result", SqlTokenType.Result );
            RegKeyword( "sets", SqlTokenType.Sets );
            RegKeyword( "undefined", SqlTokenType.Undefined );
            RegKeyword( "login", SqlTokenType.Login );
            RegKeyword( "at", SqlTokenType.At );
            RegKeyword( "using", SqlTokenType.Using );
            RegKeyword( "global", SqlTokenType.Global );
            RegKeyword( "openjson", SqlTokenType.OpenJSON );

            RegKeyword( "encryption", SqlTokenType.Encryption);
            RegKeyword( "schemabinding", SqlTokenType.SchemaBinding );
            RegKeyword( "input", SqlTokenType.Input );
            RegKeyword( "called", SqlTokenType.Called );
            RegKeyword( "native_compilation", SqlTokenType.NativeCompilation );
            RegKeyword( "server", SqlTokenType.Server );

            // LogicalOperator (they are reserved keywords).
            RegKeyword( "or", SqlTokenType.Or );
            RegKeyword( "and", SqlTokenType.And );
            RegKeyword( "not", SqlTokenType.Not );
            // CompareOperator (they are reserved keywords).
            RegKeyword( "between", SqlTokenType.Between );
            RegKeyword( "in", SqlTokenType.In );
            RegKeyword( "is", SqlTokenType.Is );
            RegKeyword( "like", SqlTokenType.Like );
            // Select operators (they are reserved keywords).
            RegKeyword( "union", SqlTokenType.Union );
            RegKeyword( "intersect", SqlTokenType.Intersect );
            RegKeyword( "except", SqlTokenType.Except );
            RegKeyword( "order", SqlTokenType.Order );
            RegKeyword( "for", SqlTokenType.For );

            // SqlTokenType.IdentifierReserved values.
            RegKeyword( "case", SqlTokenType.Case );
            RegKeyword( "null", SqlTokenType.Null );
            RegKeyword( "when", SqlTokenType.When );
            RegKeyword( "by", SqlTokenType.By );
            RegKeyword( "all", SqlTokenType.All );
            RegKeyword( "then", SqlTokenType.Then );
            RegKeyword( "else", SqlTokenType.Else );
            RegKeyword( "transaction", SqlTokenType.Transaction );
            _keywords.Add( "tran", SqlTokenType.Transaction );
            RegKeyword( "with", SqlTokenType.With );
            RegKeyword( "procedure", SqlTokenType.Procedure );
            _keywords.Add( "proc", SqlTokenType.Procedure );
            RegKeyword( "function", SqlTokenType.Function );
            RegKeyword( "view", SqlTokenType.View );
            RegKeyword( "trigger", SqlTokenType.Trigger );
            RegKeyword( "as", SqlTokenType.As );
            RegKeyword( "asc", SqlTokenType.Asc );
            RegKeyword( "desc", SqlTokenType.Desc );
            RegKeyword( "exists", SqlTokenType.Exists );
            RegKeyword( "on", SqlTokenType.On );
            RegKeyword( "to", SqlTokenType.To );
            RegKeyword( "of", SqlTokenType.Of );
            RegKeyword( "top", SqlTokenType.Top );
            RegKeyword( "escape", SqlTokenType.Escape );
            RegKeyword( "into",  SqlTokenType.Into );
            RegKeyword( "from",  SqlTokenType.From );
            RegKeyword( "where", SqlTokenType.Where );
            RegKeyword( "group", SqlTokenType.Group );
            RegKeyword( "option", SqlTokenType.Option );
            RegKeyword( "add", SqlTokenType.Add );
            RegKeyword( "database", SqlTokenType.Database );
            RegKeyword( "external", SqlTokenType.External );
            RegKeyword( "over", SqlTokenType.Over );
            RegKeyword( "cross", SqlTokenType.Cross );
            RegKeyword( "foreign", SqlTokenType.Foreign );
            RegKeyword( "clustered", SqlTokenType.Clustered );
            RegKeyword( "left", SqlTokenType.Left );
            RegKeyword( "percent", SqlTokenType.Percent );
            RegKeyword( "values", SqlTokenType.Values );
            RegKeyword( "distinct", SqlTokenType.Distinct );
            RegKeyword( "pivot", SqlTokenType.Pivot );
            RegKeyword( "having", SqlTokenType.Having );
            RegKeyword( "cursor", SqlTokenType.Cursor );
            RegKeyword( "read", SqlTokenType.Read );
            RegKeyword( "browse", SqlTokenType.Browse );
            RegKeyword( "collate", SqlTokenType.Collate );
            RegKeyword( "opendatasource", SqlTokenType.OpenDataSource );
            RegKeyword( "openrowset", SqlTokenType.OpenRowSet );
            RegKeyword( "openxml", SqlTokenType.OpenXml );
            RegKeyword( "openquery", SqlTokenType.OpenQuery );
            RegKeyword( "default", SqlTokenType.Default );
            RegKeyword( "user", SqlTokenType.User );
            RegKeyword( "current", SqlTokenType.Current );
            RegKeyword( "varying", SqlTokenType.Varying );

            RegKeyword( "freetext", SqlTokenType.FreeText );
            RegKeyword( "freetexttable", SqlTokenType.FreeTextTable );
            RegKeyword( "reconfigure", SqlTokenType.Reconfigure );
            RegKeyword( "references", SqlTokenType.References );
            RegKeyword( "full", SqlTokenType.Full );
            RegKeyword( "replication", SqlTokenType.Replication );
            RegKeyword( "bulk", SqlTokenType.Bulk );
            RegKeyword( "check", SqlTokenType.Check );
            RegKeyword( "holdlock", SqlTokenType.HoldLock );
            RegKeyword( "right", SqlTokenType.Right );
            RegKeyword( "checkpoint", SqlTokenType.Checkpoint );
            RegKeyword( "identity", SqlTokenType.Identity );
            RegKeyword( "identity_insert", SqlTokenType.IdentityInsert );
            RegKeyword( "rowcount", SqlTokenType.RowCount );
            RegKeyword( "save", SqlTokenType.Save );
            RegKeyword( "column", SqlTokenType.Column );
            RegKeyword( "index", SqlTokenType.Index );
            RegKeyword( "schema", SqlTokenType.Schema );
            RegKeyword( "inner", SqlTokenType.Inner );
            RegKeyword( "securityaudit", SqlTokenType.SecurityAudit );
            RegKeyword( "compute", SqlTokenType.Compute );
            RegKeyword( "constraint", SqlTokenType.Constraint );
            RegKeyword( "session_user", SqlTokenType.SessionUser );
            RegKeyword( "setuser", SqlTokenType.SetUser );
            RegKeyword( "join", SqlTokenType.Join );
            RegKeyword( "shutdown", SqlTokenType.Shutdown );
            RegKeyword( "convert", SqlTokenType.Convert );
            RegKeyword( "key", SqlTokenType.Key );
            RegKeyword( "statistics", SqlTokenType.Statistics );
            RegKeyword( "system_user", SqlTokenType.SystemUser );
            RegKeyword( "current_date", SqlTokenType.CurrentDate );
            RegKeyword( "lineno", SqlTokenType.LineNo );
            RegKeyword( "tablesample", SqlTokenType.TableSample );
            RegKeyword( "current_time", SqlTokenType.CurrentTime );
            RegKeyword( "load", SqlTokenType.Load );
            RegKeyword( "textsize", SqlTokenType.TextSize );
            RegKeyword( "current_timestamp", SqlTokenType.CurrentTimestamp );
            RegKeyword( "current_user", SqlTokenType.CurrentUser );
            RegKeyword( "national", SqlTokenType.National );
            RegKeyword( "nullif", SqlTokenType.NullIf );
            RegKeyword( "tsequal", SqlTokenType.TSEqual );
            RegKeyword( "off", SqlTokenType.Off );
            RegKeyword( "offsets", SqlTokenType.Offsets );
            RegKeyword( "unique", SqlTokenType.Unique );
            RegKeyword( "unpivot", SqlTokenType.Unpivot );
            RegKeyword( "disk", SqlTokenType.Disk );
            RegKeyword( "dump", SqlTokenType.Dump );
            RegKeyword( "errlvl", SqlTokenType.ErrLvl );
            RegKeyword( "restrict", SqlTokenType.Restrict );
            RegKeyword( "cascade", SqlTokenType.Cascade );
            RegKeyword( "revert", SqlTokenType.Revert );
            RegKeyword( "revoke", SqlTokenType.Revoke );
            RegKeyword( "any", SqlTokenType.Any );
            RegKeyword( "some", SqlTokenType.Some );
            RegKeyword( "precision", SqlTokenType.Precision );
            RegKeyword( "exit", SqlTokenType.Exit );
            RegKeyword( "primary", SqlTokenType.Primary );
            RegKeyword( "plan", SqlTokenType.Plan );
            RegKeyword( "file", SqlTokenType.File );
            RegKeyword( "fillfactor", SqlTokenType.FillFactor );
            RegKeyword( "public", SqlTokenType.Public );
            RegKeyword( "authorization", SqlTokenType.Authorization );
            RegKeyword( "distributed", SqlTokenType.Distributed );
            RegKeyword( "coalesce", SqlTokenType.Coalesce );
            RegKeyword( "rule", SqlTokenType.Rule );
            RegKeyword( "identitycol", SqlTokenType.IdentityCol );
            RegKeyword( "rowguidcol", SqlTokenType.RowguidCol );
            RegKeyword( "contains", SqlTokenType.Contains );
            RegKeyword( "containstable", SqlTokenType.ContainsTable );
            RegKeyword( "nocheck", SqlTokenType.NoCheck );
            RegKeyword( "nonclustered", SqlTokenType.NonClustered );
            RegKeyword( "double", SqlTokenType.Double );
            RegKeyword( "outer", SqlTokenType.Outer );

            // SqlTokenType.IdentifierReservedStatement values.
            RegKeyword( "select", SqlTokenType.Select );
            RegKeyword( "begin", SqlTokenType.Begin );
            RegKeyword( "end", SqlTokenType.End );
            RegKeyword( "create", SqlTokenType.Create );
            RegKeyword( "drop", SqlTokenType.Drop );
            RegKeyword( "alter", SqlTokenType.Alter );
            RegKeyword( "declare", SqlTokenType.Declare );
            RegKeyword( "break", SqlTokenType.Break );
            RegKeyword( "continue", SqlTokenType.Continue );
            RegKeyword( "goto", SqlTokenType.Goto );
            RegKeyword( "while", SqlTokenType.While );
            RegKeyword( "if", SqlTokenType.If );
            RegKeyword( "deallocate", SqlTokenType.Deallocate );
            RegKeyword( "close", SqlTokenType.Close );
            RegKeyword( "fetch", SqlTokenType.Fetch );
            RegKeyword( "open", SqlTokenType.Open );
            RegKeyword( "return", SqlTokenType.Return );
            RegKeyword( "execute", SqlTokenType.Execute );
            _keywords.Add( "exec", SqlTokenType.Execute );
            RegKeyword( "set", SqlTokenType.Set );
            RegKeyword( "update", SqlTokenType.Update );
            RegKeyword( "insert", SqlTokenType.Insert );
            RegKeyword( "raiserror", SqlTokenType.Raiserror );
            RegKeyword( "waitfor", SqlTokenType.WaitFor );
            RegKeyword( "use", SqlTokenType.Use );
            RegKeyword( "truncate", SqlTokenType.Truncate );
            RegKeyword( "print", SqlTokenType.Print );
            RegKeyword( "commit", SqlTokenType.Commit );
            RegKeyword( "rollback", SqlTokenType.Rollback );
            RegKeyword( "delete", SqlTokenType.Delete );
            RegKeyword( "updatetext", SqlTokenType.Updatetext );
            RegKeyword( "merge", SqlTokenType.Merge );
            RegKeyword( "kill", SqlTokenType.Kill );
            RegKeyword( "readtext", SqlTokenType.Readtext );
            RegKeyword( "writetext", SqlTokenType.Writetext );
            RegKeyword( "dbcc", SqlTokenType.Dbcc );
            RegKeyword( "go", SqlTokenType.Go );
            RegKeyword( "backup", SqlTokenType.Backup );
            RegKeyword( "restore", SqlTokenType.Restore );
            RegKeyword( "grant", SqlTokenType.Grant );
            RegKeyword( "deny", SqlTokenType.Deny );

            Debug.Assert( (int)SqlTokenType.AssignOperatorCount == 9 );
            _typeToString.Add( SqlTokenType.Assign, "=" );
            _typeToString.Add( SqlTokenType.BitwiseOrAssign, "|=" );
            _typeToString.Add( SqlTokenType.BitwiseAndAssign, "&=" );
            _typeToString.Add( SqlTokenType.BitwiseXOrAssign, "^=" );
            _typeToString.Add( SqlTokenType.PlusAssign, "+=" );
            _typeToString.Add( SqlTokenType.MinusAssign, "-=" );
            _typeToString.Add( SqlTokenType.DivideAssign, "/=" );
            _typeToString.Add( SqlTokenType.MultAssign, "*=" );
            _typeToString.Add( SqlTokenType.ModuloAssign, "%=" );

            Debug.Assert( (int)SqlTokenType.BasicOperatorCount == 9 );
            _typeToString.Add( SqlTokenType.BitwiseOr, "|" );
            _typeToString.Add( SqlTokenType.BitwiseXOr, "^" );
            _typeToString.Add( SqlTokenType.BitwiseAnd, "&" );
            _typeToString.Add( SqlTokenType.Plus, "+" );
            _typeToString.Add( SqlTokenType.Minus, "-" );
            _typeToString.Add( SqlTokenType.Mult, "*" );
            _typeToString.Add( SqlTokenType.Divide, "/" );
            _typeToString.Add( SqlTokenType.Modulo, "%" );
            _typeToString.Add( SqlTokenType.BitwiseNot, "~" );

            Debug.Assert( (int)SqlTokenType.CompareOperatorCount == 9 );
            _typeToString.Add( SqlTokenType.Equal, "=" );
            _typeToString.Add( SqlTokenType.Greater, ">" );
            _typeToString.Add( SqlTokenType.Less, "<" );
            _typeToString.Add( SqlTokenType.GreaterOrEqual, ">=" );
            _typeToString.Add( SqlTokenType.LessOrEqual, "<=" );
            _typeToString.Add( SqlTokenType.NotEqualTo, "<>" );
            _typeToString.Add( SqlTokenType.Different, "!=" );
            _typeToString.Add( SqlTokenType.NotGreaterThan, "!>" );
            _typeToString.Add( SqlTokenType.NotLessThan, "!<" );

            Debug.Assert( (int)SqlTokenType.PunctuationCount == 5 );
            _typeToString.Add( SqlTokenType.Dot, "." );
            _typeToString.Add( SqlTokenType.Comma, "," );
            _typeToString.Add( SqlTokenType.SemiColon, ";" );
            _typeToString.Add( SqlTokenType.Colon, ":" );
            _typeToString.Add( SqlTokenType.DoubleColons, "::" );

            _typeToString.Add( SqlTokenType.OpenPar, "(" );
            _typeToString.Add( SqlTokenType.ClosePar, ")" );
            _typeToString.Add( SqlTokenType.OpenBracket, "[" );
            _typeToString.Add( SqlTokenType.CloseBracket, "]" );
            _typeToString.Add( SqlTokenType.OpenCurly, "{" );
            _typeToString.Add( SqlTokenType.CloseCurly, "}" );

            _typeToString.Add( SqlTokenType.IdentifierStar, "*" );

            _typeToString.Add( SqlTokenType.None, "刁one" );
            _typeToString.Add( SqlTokenType.ErrorMask, "九rror" );
            _typeToString.Add( SqlTokenType.EndOfInput, "九ndOfInput" );
            _typeToString.Add( SqlTokenType.ErrorInvalidChar, "九rrorInvalidChar" );
            _typeToString.Add( SqlTokenType.ErrorStringUnterminated, "九rrorStringUnterminated" );
            _typeToString.Add( SqlTokenType.ErrorIdentifierUnterminated, "九rrorIdentifierUnterminated" );
            _typeToString.Add( SqlTokenType.ErrorNumberUnterminatedValue, "九rrorNumberUnterminatedValue" );
            _typeToString.Add( SqlTokenType.ErrorNumberValue, "九rrorNumberValue" );
            _typeToString.Add( SqlTokenType.ErrorNumberIdentifierStartsImmediately, "九rrorNumberIdentifierStartsImmediately" );

            _typeToString.Add( SqlTokenType.String, "又tring" );
            _typeToString.Add( SqlTokenType.UnicodeString, "下nicodeString" );
            _typeToString.Add( SqlTokenType.Integer, "儿nteger" );
            _typeToString.Add( SqlTokenType.Binary, "丁inary" );
            _typeToString.Add( SqlTokenType.Float, "了loat" );
            _typeToString.Add( SqlTokenType.Decimal, "乃ecimal" );
            _typeToString.Add( SqlTokenType.Money, "刀oney" );
            _typeToString.Add( SqlTokenType.StarComment, "又tarComment" );
            _typeToString.Add( SqlTokenType.LineComment, "几ineComment" );

            _typeToString.Add( SqlTokenType.IdentifierStandard, "儿dentifierStandard" );
            _typeToString.Add( SqlTokenType.IdentifierQuoted, "儿dentifierQuoted" );
            _typeToString.Add( SqlTokenType.IdentifierQuotedBracket, "儿dentifierQuotedBracket" );
            _typeToString.Add( SqlTokenType.IdentifierVariable, "儿dentifierVariable" );
            _typeToString.Add( SqlTokenType.IdentifierSpecial, "儿dentifierSpecial" );
        }

        [Conditional("DEBUG")]
        internal static void CheckTokenTypeStringMapping( SqlTokenType t )
        {
            Debug.Assert( _typeToString.ContainsKey( t ), "SqlTokenType not mapped: " + t.ToString() );
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

        public static string ToString( SqlTokenType t )
        {
            return _typeToString[t];
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
