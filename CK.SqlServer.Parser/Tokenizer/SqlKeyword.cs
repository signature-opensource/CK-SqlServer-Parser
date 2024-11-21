using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.SqlServer.Parser;

public static class SqlKeyword
{
    public static readonly SqlTokenError BegOfInput;
    public static readonly SqlTokenError EndOfInput;

    public static readonly SqlTokenDot Dot;
    public static readonly SqlTokenDoubleColon DoubleColon;
    public static readonly SqlTokenMultiDots DoubleDots;
    public static readonly SqlTokenMultiDots TripleDots;
    public static readonly SqlTokenComma Comma;
    public static readonly SqlTokenComma CommaOneSpace;
    public static readonly SqlTokenTerminal SemiColon;
    public static readonly SqlTokenTerminal QuestionMark;
    public static readonly SqlTokenTerminal DoubleQuestionMark;
    public static readonly SqlTokenTerminal TripleQuestionMark;
    public static readonly SqlTokenTerminal QuadrupleQuestionMark;
    public static readonly SqlTokenOpenPar OpenPar;
    public static readonly SqlTokenClosePar ClosePar;
    public static readonly SqlTokenTerminal Colon;
    public static readonly SqlTokenTerminal OpenBracket;
    public static readonly SqlTokenTerminal CloseBracket;
    public static readonly SqlTokenTerminal OpenCurly;
    public static readonly SqlTokenTerminal CloseCurly;
    public static readonly SqlTokenTerminal OpenCurlyInCurly;
    public static readonly SqlTokenTerminal CloseCurlyInCurly;

    public static readonly SqlTokenTerminal Assign;
    public static readonly SqlTokenTerminal BitwiseOrAssign;
    public static readonly SqlTokenTerminal BitwiseAndAssign;
    public static readonly SqlTokenTerminal BitwiseXOrAssign;
    public static readonly SqlTokenTerminal PlusAssign;
    public static readonly SqlTokenTerminal MinusAssign;
    public static readonly SqlTokenTerminal DivideAssign;
    public static readonly SqlTokenTerminal MultAssign;
    public static readonly SqlTokenTerminal ModuloAssign;
    public static readonly SqlTokenTerminal BitwiseOr;
    public static readonly SqlTokenTerminal BitwiseXOr;
    public static readonly SqlTokenTerminal BitwiseAnd;
    public static readonly SqlTokenTerminal Plus;
    public static readonly SqlTokenTerminal Minus;
    public static readonly SqlTokenTerminal Mult;
    public static readonly SqlTokenTerminal Divide;
    public static readonly SqlTokenTerminal Modulo;
    public static readonly SqlTokenTerminal BitwiseNot;
    public static readonly SqlTokenTerminal Equal;
    public static readonly SqlTokenTerminal Greater;
    public static readonly SqlTokenTerminal Less;
    public static readonly SqlTokenTerminal GreaterOrEqual;
    public static readonly SqlTokenTerminal LessOrEqual;
    public static readonly SqlTokenTerminal NotEqualTo;
    public static readonly SqlTokenTerminal Different;
    public static readonly SqlTokenTerminal NotGreaterThan;
    public static readonly SqlTokenTerminal NotLessThan;

    public static readonly SqlTokenIdentifier IdentifierStar;


    public static readonly SqlTokenIdentifier SqlVariant;
    public static readonly SqlTokenIdentifier Xml;
    public static readonly SqlTokenIdentifier DateTimeOffset;
    public static readonly SqlTokenIdentifier DateTime2;
    public static readonly SqlTokenIdentifier DateTime;
    public static readonly SqlTokenIdentifier SmallDateTime;
    public static readonly SqlTokenIdentifier Date;
    public static readonly SqlTokenIdentifier Time;
    public static readonly SqlTokenIdentifier Float;
    public static readonly SqlTokenIdentifier Real;
    public static readonly SqlTokenIdentifier Decimal;
    public static readonly SqlTokenIdentifier Money;
    public static readonly SqlTokenIdentifier SmallMoney;
    public static readonly SqlTokenIdentifier BigInt;
    public static readonly SqlTokenIdentifier Int;
    public static readonly SqlTokenIdentifier SmallInt;
    public static readonly SqlTokenIdentifier TinyInt;
    public static readonly SqlTokenIdentifier Bit;
    public static readonly SqlTokenIdentifier NText;
    public static readonly SqlTokenIdentifier Text;
    public static readonly SqlTokenIdentifier Image;
    public static readonly SqlTokenIdentifier TimeStamp;
    public static readonly SqlTokenIdentifier UniqueIdentifier;
    public static readonly SqlTokenIdentifier NVarChar;
    public static readonly SqlTokenIdentifier NChar;
    public static readonly SqlTokenIdentifier VarChar;
    public static readonly SqlTokenIdentifier Char;
    public static readonly SqlTokenIdentifier VarBinary;
    public static readonly SqlTokenIdentifier Binary;
    public static readonly SqlTokenIdentifier Table;


    public static readonly SqlTokenIdentifier Throw;
    public static readonly SqlTokenIdentifier Get;
    public static readonly SqlTokenIdentifier Move;
    public static readonly SqlTokenIdentifier Receive;
    public static readonly SqlTokenIdentifier Send;
    public static readonly SqlTokenIdentifier Try;
    public static readonly SqlTokenIdentifier Catch;
    public static readonly SqlTokenIdentifier Dialog;
    public static readonly SqlTokenIdentifier Conversation;
    public static readonly SqlTokenIdentifier Returns;
    public static readonly SqlTokenIdentifier Max;
    public static readonly SqlTokenIdentifier insensitive;
    public static readonly SqlTokenIdentifier Scroll;
    public static readonly SqlTokenIdentifier Mark;
    public static readonly SqlTokenIdentifier JSON;
    public static readonly SqlTokenIdentifier SystemTime;
    public static readonly SqlTokenIdentifier Ties;
    public static readonly SqlTokenIdentifier ReadOnly;
    public static readonly SqlTokenIdentifier Output;
    public static readonly SqlTokenIdentifier Rows;
    public static readonly SqlTokenIdentifier Offset;
    public static readonly SqlTokenIdentifier First;
    public static readonly SqlTokenIdentifier Next;
    public static readonly SqlTokenIdentifier Only;
    public static readonly SqlTokenIdentifier Cast;
    public static readonly SqlTokenIdentifier Value;
    public static readonly SqlTokenIdentifier Matched;
    public static readonly SqlTokenIdentifier Recompile;
    public static readonly SqlTokenIdentifier Result;
    public static readonly SqlTokenIdentifier Sets;
    public static readonly SqlTokenIdentifier Undefined;
    public static readonly SqlTokenIdentifier Login;
    public static readonly SqlTokenIdentifier At;
    public static readonly SqlTokenIdentifier Zone;
    public static readonly SqlTokenIdentifier Using;
    public static readonly SqlTokenIdentifier Global;
    public static readonly SqlTokenIdentifier OpenJSON;
    public static readonly SqlTokenIdentifier Encryption;
    public static readonly SqlTokenIdentifier SchemaBinding;
    public static readonly SqlTokenIdentifier Input;
    public static readonly SqlTokenIdentifier Called;
    public static readonly SqlTokenIdentifier NativeCompilation;
    public static readonly SqlTokenIdentifier Server;
    public static readonly SqlTokenIdentifier Last;
    public static readonly SqlTokenIdentifier XmlNamespaces;

    public static readonly SqlTokenIdentifier Or;
    public static readonly SqlTokenIdentifier And;
    public static readonly SqlTokenIdentifier Not;
    public static readonly SqlTokenIdentifier Between;
    public static readonly SqlTokenIdentifier In;
    public static readonly SqlTokenIdentifier Is;
    public static readonly SqlTokenIdentifier Like;
    public static readonly SqlTokenIdentifier Union;
    public static readonly SqlTokenIdentifier Intersect;
    public static readonly SqlTokenIdentifier Except;
    public static readonly SqlTokenIdentifier Order;
    public static readonly SqlTokenIdentifier For;
    public static readonly SqlTokenIdentifier Case;
    public static readonly SqlTokenIdentifier Null;
    public static readonly SqlTokenIdentifier When;
    public static readonly SqlTokenIdentifier By;
    public static readonly SqlTokenIdentifier All;
    public static readonly SqlTokenIdentifier Then;
    public static readonly SqlTokenIdentifier Else;
    public static readonly SqlTokenIdentifier Transaction;
    public static readonly SqlTokenIdentifier With;
    public static readonly SqlTokenIdentifier Procedure;
    public static readonly SqlTokenIdentifier Function;
    public static readonly SqlTokenIdentifier View;
    public static readonly SqlTokenIdentifier Trigger;
    public static readonly SqlTokenIdentifier As;
    public static readonly SqlTokenIdentifier Asc;
    public static readonly SqlTokenIdentifier Desc;
    public static readonly SqlTokenIdentifier Exists;
    public static readonly SqlTokenIdentifier On;
    public static readonly SqlTokenIdentifier To;
    public static readonly SqlTokenIdentifier Of;
    public static readonly SqlTokenIdentifier Top;
    public static readonly SqlTokenIdentifier Escape;
    public static readonly SqlTokenIdentifier Where;
    public static readonly SqlTokenIdentifier Into;
    public static readonly SqlTokenIdentifier From;
    public static readonly SqlTokenIdentifier Group;
    public static readonly SqlTokenIdentifier Option;
    public static readonly SqlTokenIdentifier Add;
    public static readonly SqlTokenIdentifier Database;
    public static readonly SqlTokenIdentifier External;
    public static readonly SqlTokenIdentifier Over;
    public static readonly SqlTokenIdentifier Cross;
    public static readonly SqlTokenIdentifier Foreign;
    public static readonly SqlTokenIdentifier Clustered;
    public static readonly SqlTokenIdentifier Left;
    public static readonly SqlTokenIdentifier Percent;
    public static readonly SqlTokenIdentifier Values;
    public static readonly SqlTokenIdentifier Distinct;
    public static readonly SqlTokenIdentifier Pivot;
    public static readonly SqlTokenIdentifier Having;
    public static readonly SqlTokenIdentifier Cursor;
    public static readonly SqlTokenIdentifier Read;
    public static readonly SqlTokenIdentifier Browse;
    public static readonly SqlTokenIdentifier Collate;
    public static readonly SqlTokenIdentifier OpenDataSource;
    public static readonly SqlTokenIdentifier OpenRowSet;
    public static readonly SqlTokenIdentifier OpenXml;
    public static readonly SqlTokenIdentifier OpenQuery;
    public static readonly SqlTokenIdentifier Default;
    public static readonly SqlTokenIdentifier User;
    public static readonly SqlTokenIdentifier Current;
    public static readonly SqlTokenIdentifier Varying;
    public static readonly SqlTokenIdentifier FreeText;
    public static readonly SqlTokenIdentifier FreeTextTable;
    public static readonly SqlTokenIdentifier Reconfigure;
    public static readonly SqlTokenIdentifier References;
    public static readonly SqlTokenIdentifier Full;
    public static readonly SqlTokenIdentifier Replication;
    public static readonly SqlTokenIdentifier Bulk;
    public static readonly SqlTokenIdentifier Check;
    public static readonly SqlTokenIdentifier HoldLock;
    public static readonly SqlTokenIdentifier Right;
    public static readonly SqlTokenIdentifier Checkpoint;
    public static readonly SqlTokenIdentifier Identity;
    public static readonly SqlTokenIdentifier IdentityInsert;
    public static readonly SqlTokenIdentifier RowCount;
    public static readonly SqlTokenIdentifier Save;
    public static readonly SqlTokenIdentifier Column;
    public static readonly SqlTokenIdentifier Index;
    public static readonly SqlTokenIdentifier Schema;
    public static readonly SqlTokenIdentifier Inner;
    public static readonly SqlTokenIdentifier SecurityAudit;
    public static readonly SqlTokenIdentifier Compute;
    public static readonly SqlTokenIdentifier Constraint;
    public static readonly SqlTokenIdentifier SessionUser;
    public static readonly SqlTokenIdentifier Within;
    public static readonly SqlTokenIdentifier Preceding;
    public static readonly SqlTokenIdentifier Unbounded;
    public static readonly SqlTokenIdentifier Following;
    public static readonly SqlTokenIdentifier Partition;
    public static readonly SqlTokenIdentifier SetUser;
    public static readonly SqlTokenIdentifier Join;
    public static readonly SqlTokenIdentifier Shutdown;
    public static readonly SqlTokenIdentifier Convert;
    public static readonly SqlTokenIdentifier Key;
    public static readonly SqlTokenIdentifier Statistics;
    public static readonly SqlTokenIdentifier SystemUser;
    public static readonly SqlTokenIdentifier CurrentDate;
    public static readonly SqlTokenIdentifier LineNo;
    public static readonly SqlTokenIdentifier TableSample;
    public static readonly SqlTokenIdentifier CurrentTime;
    public static readonly SqlTokenIdentifier Load;
    public static readonly SqlTokenIdentifier TextSize;
    public static readonly SqlTokenIdentifier CurrentTimestamp;
    public static readonly SqlTokenIdentifier CurrentUser;
    public static readonly SqlTokenIdentifier National;
    public static readonly SqlTokenIdentifier NullIf;
    public static readonly SqlTokenIdentifier TSEqual;
    public static readonly SqlTokenIdentifier Off;
    public static readonly SqlTokenIdentifier Offsets;
    public static readonly SqlTokenIdentifier Unique;
    public static readonly SqlTokenIdentifier Unpivot;
    public static readonly SqlTokenIdentifier Disk;
    public static readonly SqlTokenIdentifier Dump;
    public static readonly SqlTokenIdentifier ErrLvl;
    public static readonly SqlTokenIdentifier Restrict;
    public static readonly SqlTokenIdentifier Cascade;
    public static readonly SqlTokenIdentifier Revert;
    public static readonly SqlTokenIdentifier Revoke;
    public static readonly SqlTokenIdentifier Any;
    public static readonly SqlTokenIdentifier Some;
    public static readonly SqlTokenIdentifier Precision;
    public static readonly SqlTokenIdentifier Exit;
    public static readonly SqlTokenIdentifier Primary;
    public static readonly SqlTokenIdentifier Plan;
    public static readonly SqlTokenIdentifier File;
    public static readonly SqlTokenIdentifier FillFactor;
    public static readonly SqlTokenIdentifier Public;
    public static readonly SqlTokenIdentifier Authorization;
    public static readonly SqlTokenIdentifier Distributed;
    public static readonly SqlTokenIdentifier Coalesce;
    public static readonly SqlTokenIdentifier Rule;
    public static readonly SqlTokenIdentifier IdentityCol;
    public static readonly SqlTokenIdentifier RowguidCol;
    public static readonly SqlTokenIdentifier Contains;
    public static readonly SqlTokenIdentifier ContainsTable;
    public static readonly SqlTokenIdentifier NoCheck;
    public static readonly SqlTokenIdentifier NonClustered;
    public static readonly SqlTokenIdentifier Double;
    public static readonly SqlTokenIdentifier Outer;
    public static readonly SqlTokenIdentifier Select;
    public static readonly SqlTokenIdentifier Begin;
    public static readonly SqlTokenIdentifier End;
    public static readonly SqlTokenIdentifier Create;
    public static readonly SqlTokenIdentifier Drop;
    public static readonly SqlTokenIdentifier Alter;
    public static readonly SqlTokenIdentifier Declare;
    public static readonly SqlTokenIdentifier Break;
    public static readonly SqlTokenIdentifier Continue;
    public static readonly SqlTokenIdentifier Goto;
    public static readonly SqlTokenIdentifier While;
    public static readonly SqlTokenIdentifier If;
    public static readonly SqlTokenIdentifier Deallocate;
    public static readonly SqlTokenIdentifier Close;
    public static readonly SqlTokenIdentifier Fetch;
    public static readonly SqlTokenIdentifier Open;
    public static readonly SqlTokenIdentifier Return;
    public static readonly SqlTokenIdentifier Execute;
    public static readonly SqlTokenIdentifier Set;
    public static readonly SqlTokenIdentifier Update;
    public static readonly SqlTokenIdentifier Insert;
    public static readonly SqlTokenIdentifier Raiserror;
    public static readonly SqlTokenIdentifier WaitFor;
    public static readonly SqlTokenIdentifier Use;
    public static readonly SqlTokenIdentifier Truncate;
    public static readonly SqlTokenIdentifier Print;
    public static readonly SqlTokenIdentifier Commit;
    public static readonly SqlTokenIdentifier Rollback;
    public static readonly SqlTokenIdentifier Delete;
    public static readonly SqlTokenIdentifier Updatetext;
    public static readonly SqlTokenIdentifier Merge;
    public static readonly SqlTokenIdentifier Kill;
    public static readonly SqlTokenIdentifier Readtext;
    public static readonly SqlTokenIdentifier Writetext;
    public static readonly SqlTokenIdentifier Dbcc;
    public static readonly SqlTokenIdentifier Go;
    public static readonly SqlTokenIdentifier Backup;
    public static readonly SqlTokenIdentifier Restore;
    public static readonly SqlTokenIdentifier Grant;
    public static readonly SqlTokenIdentifier Deny;

    // Transformer language extension...
    public static readonly SqlTokenIdentifier Transformer;
    public static readonly SqlTokenIdentifier Parameter;
    public static readonly SqlTokenIdentifier After;
    public static readonly SqlTokenIdentifier Before;
    public static readonly SqlTokenIdentifier Around;
    public static readonly SqlTokenIdentifier Inject;
    public static readonly SqlTokenIdentifier Each;
    public static readonly SqlTokenIdentifier Single;
    public static readonly SqlTokenIdentifier Replace;
    public static readonly SqlTokenIdentifier Scope;
    //public static readonly SqlTokenIdentifier Largest;
    //public static readonly SqlTokenIdentifier Deepest;
    //public static readonly SqlTokenIdentifier Nodes;
    public static readonly SqlTokenIdentifier Part;
    public static readonly SqlTokenIdentifier Statement;
    public static readonly SqlTokenIdentifier Range;
    public static readonly SqlTokenIdentifier Combine;
    public static readonly SqlTokenIdentifier Out;

    static Dictionary<string, SqlTokenType> _keywords;
    static Dictionary<SqlTokenType, string> _typeToString;

    static SqlTokenIdentifier RegKeyword( string name, SqlTokenType t )
    {
        _keywords.Add( name, t );
        Debug.Assert( !_typeToString.ContainsKey( t ) );
        _typeToString.Add( t, name );
        return new SqlTokenIdentifier( t, name, null, null );
    }

    static SqlTokenTerminal RegTerminal( SqlTokenType t, string value )
    {
        Debug.Assert( t > 0 && (t &
            (SqlTokenType.IsAssignOperator
            | SqlTokenType.IsCompareOperator
            | SqlTokenType.IsBasicOperator
            | SqlTokenType.IsPunctuation
            | SqlTokenType.IsBracket)) != 0 );
        _typeToString.Add( t, value );
        return SqlTokenTerminal.Create( t, null, null );
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
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.XmlDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Xml );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.DateTimeOffsetDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.DateTimeOffset );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.DateTime2DbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.DateTime2 );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.DateTimeDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.DateTime );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.SmallDateTimeDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.SmallDateTime );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.DateDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Date );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.TimeDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Time );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.FloatDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Float );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.RealDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Real );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.DecimalDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Decimal );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.MoneyDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Money );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.SmallMoneyDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.SmallMoney );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.BigIntDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.BigInt );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.IntDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Int );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.SmallIntDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.SmallInt );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.TinyIntDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.TinyInt );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.BitDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Bit );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.NTextDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.NText );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.TextDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Text );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.ImageDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Image );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.TimestampDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Timestamp );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.UniqueIdentifierDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.UniqueIdentifier );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.NVarCharDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.NVarChar );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.NCharDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.NChar );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.VarCharDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.VarChar );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.CharDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Char );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.VarBinaryDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.VarBinary );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.BinaryDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Binary );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.VariantDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Variant );
        Debug.Assert( _sqlDbTypesMapped[(int)(SqlTokenType.TableDbType & SqlTokenType.IdentifierValueMask)] == SqlDbType.Structured );

        _keywords = new Dictionary<string, SqlTokenType>( StringComparer.OrdinalIgnoreCase );
        _typeToString = new Dictionary<SqlTokenType, string>();

        // Identifiers mapped to SqlTokenType.

        // SqlDbType mapping.
        SqlVariant = RegKeyword( "sql_variant", SqlTokenType.VariantDbType );
        Xml = RegKeyword( "xml", SqlTokenType.XmlDbType );
        DateTimeOffset = RegKeyword( "datetimeoffset", SqlTokenType.DateTimeOffsetDbType );
        DateTime2 = RegKeyword( "datetime2", SqlTokenType.DateTime2DbType );
        DateTime = RegKeyword( "datetime", SqlTokenType.DateTimeDbType );
        SmallDateTime = RegKeyword( "smalldatetime", SqlTokenType.SmallDateTimeDbType );
        Date = RegKeyword( "date", SqlTokenType.DateDbType );
        Time = RegKeyword( "time", SqlTokenType.TimeDbType );
        Float = RegKeyword( "float", SqlTokenType.FloatDbType );
        Real = RegKeyword( "real", SqlTokenType.RealDbType );
        Decimal = RegKeyword( "decimal", SqlTokenType.DecimalDbType );
        _keywords.Add( "numeric", SqlTokenType.DecimalDbType );
        Money = RegKeyword( "money", SqlTokenType.MoneyDbType );
        SmallMoney = RegKeyword( "smallmoney", SqlTokenType.SmallMoneyDbType );
        BigInt = RegKeyword( "bigint", SqlTokenType.BigIntDbType );
        Int = RegKeyword( "int", SqlTokenType.IntDbType );
        SmallInt = RegKeyword( "smallint", SqlTokenType.SmallIntDbType );
        TinyInt = RegKeyword( "tinyint", SqlTokenType.TinyIntDbType );
        Bit = RegKeyword( "bit", SqlTokenType.BitDbType );
        NText = RegKeyword( "ntext", SqlTokenType.NTextDbType );
        Text = RegKeyword( "text", SqlTokenType.TextDbType );
        Image = RegKeyword( "image", SqlTokenType.ImageDbType );
        TimeStamp = RegKeyword( "timestamp", SqlTokenType.TimestampDbType );
        UniqueIdentifier = RegKeyword( "uniqueidentifier", SqlTokenType.UniqueIdentifierDbType );
        NVarChar = RegKeyword( "nvarchar", SqlTokenType.NVarCharDbType );
        NChar = RegKeyword( "nchar", SqlTokenType.NCharDbType );
        VarChar = RegKeyword( "varchar", SqlTokenType.VarCharDbType );
        Char = RegKeyword( "char", SqlTokenType.CharDbType );
        VarBinary = RegKeyword( "varbinary", SqlTokenType.VarBinaryDbType );
        Binary = RegKeyword( "binary", SqlTokenType.BinaryDbType );
        Table = RegKeyword( "table", SqlTokenType.TableDbType );

        Debug.Assert( _keywords.Values.All( t => !t.IsReservedKeyword()
                                                || t == SqlTokenType.TableDbType ),
                                        "Sql database type names are not reserved keyworkds except 'table'." );

        // SqlTokenType.IdentifierStandardStatement values: these are not reserved keywords but they can start a statement.
        Throw = RegKeyword( "throw", SqlTokenType.Throw );
        Get = RegKeyword( "get", SqlTokenType.Get );
        Move = RegKeyword( "move", SqlTokenType.Move );
        Receive = RegKeyword( "receive", SqlTokenType.Receive );
        Send = RegKeyword( "send", SqlTokenType.Send );

        // SqlTokenType.IdentifierStandard values: these are not reserved keywords.
        Try = RegKeyword( "try", SqlTokenType.Try );
        Catch = RegKeyword( "catch", SqlTokenType.Catch );
        Dialog = RegKeyword( "dialog", SqlTokenType.Dialog );
        Conversation = RegKeyword( "conversation", SqlTokenType.Conversation );
        Returns = RegKeyword( "returns", SqlTokenType.Returns );
        Max = RegKeyword( "max", SqlTokenType.Max );
        insensitive = RegKeyword( "insensitive", SqlTokenType.Insensitive );
        Scroll = RegKeyword( "scroll", SqlTokenType.Scroll );
        Mark = RegKeyword( "mark", SqlTokenType.Mark );
        JSON = RegKeyword( "json", SqlTokenType.Json );
        SystemTime = RegKeyword( "system_time", SqlTokenType.SystemTime );
        Ties = RegKeyword( "ties", SqlTokenType.Ties );
        ReadOnly = RegKeyword( "readonly", SqlTokenType.Readonly );
        Out = RegKeyword( "out", SqlTokenType.Out );
        Output = RegKeyword( "output", SqlTokenType.Output );
        Rows = RegKeyword( "rows", SqlTokenType.Rows );
        _keywords.Add( "row", SqlTokenType.Rows );
        Offset = RegKeyword( "offset", SqlTokenType.Offset );
        First = RegKeyword( "first", SqlTokenType.First );
        Next = RegKeyword( "next", SqlTokenType.Next );
        Only = RegKeyword( "only", SqlTokenType.Only );
        Cast = RegKeyword( "cast", SqlTokenType.Cast );
        Value = RegKeyword( "value", SqlTokenType.Value );
        Matched = RegKeyword( "matched", SqlTokenType.Matched );
        Recompile = RegKeyword( "recompile", SqlTokenType.Recompile );
        Result = RegKeyword( "result", SqlTokenType.Result );
        Sets = RegKeyword( "sets", SqlTokenType.Sets );
        Undefined = RegKeyword( "undefined", SqlTokenType.Undefined );
        Login = RegKeyword( "login", SqlTokenType.Login );
        At = RegKeyword( "at", SqlTokenType.At );
        Using = RegKeyword( "using", SqlTokenType.Using );
        Global = RegKeyword( "global", SqlTokenType.Global );
        OpenJSON = RegKeyword( "openjson", SqlTokenType.OpenJSON );

        Encryption = RegKeyword( "encryption", SqlTokenType.Encryption );
        SchemaBinding = RegKeyword( "schemabinding", SqlTokenType.SchemaBinding );
        Input = RegKeyword( "input", SqlTokenType.Input );
        Called = RegKeyword( "called", SqlTokenType.Called );
        NativeCompilation = RegKeyword( "native_compilation", SqlTokenType.NativeCompilation );
        Server = RegKeyword( "server", SqlTokenType.Server );
        Last = RegKeyword( "last", SqlTokenType.Last );
        XmlNamespaces = RegKeyword( "xmlnamespaces", SqlTokenType.XmlNamespaces );
        Zone = RegKeyword( "zone", SqlTokenType.Zone );

        // LogicalOperator (they are reserved keywords).
        Or = RegKeyword( "or", SqlTokenType.Or );
        And = RegKeyword( "and", SqlTokenType.And );
        Not = RegKeyword( "not", SqlTokenType.Not );
        // CompareOperator (they are reserved keywords).
        Between = RegKeyword( "between", SqlTokenType.Between );
        In = RegKeyword( "in", SqlTokenType.In );
        Is = RegKeyword( "is", SqlTokenType.Is );
        Like = RegKeyword( "like", SqlTokenType.Like );
        // Select operators (they are reserved keywords).
        Union = RegKeyword( "union", SqlTokenType.Union );
        Intersect = RegKeyword( "intersect", SqlTokenType.Intersect );
        Except = RegKeyword( "except", SqlTokenType.Except );
        Order = RegKeyword( "order", SqlTokenType.Order );
        For = RegKeyword( "for", SqlTokenType.For );

        // SqlTokenType.IdentifierReserved values.
        Case = RegKeyword( "case", SqlTokenType.Case );
        Null = RegKeyword( "null", SqlTokenType.Null );
        When = RegKeyword( "when", SqlTokenType.When );
        By = RegKeyword( "by", SqlTokenType.By );
        All = RegKeyword( "all", SqlTokenType.All );
        Then = RegKeyword( "then", SqlTokenType.Then );
        Else = RegKeyword( "else", SqlTokenType.Else );
        Transaction = RegKeyword( "transaction", SqlTokenType.Transaction );
        _keywords.Add( "tran", SqlTokenType.Transaction );
        With = RegKeyword( "with", SqlTokenType.With );
        Procedure = RegKeyword( "procedure", SqlTokenType.Procedure );
        _keywords.Add( "proc", SqlTokenType.Procedure );
        Function = RegKeyword( "function", SqlTokenType.Function );
        View = RegKeyword( "view", SqlTokenType.View );
        Trigger = RegKeyword( "trigger", SqlTokenType.Trigger );
        As = RegKeyword( "as", SqlTokenType.As );
        Asc = RegKeyword( "asc", SqlTokenType.Asc );
        Desc = RegKeyword( "desc", SqlTokenType.Desc );
        Exists = RegKeyword( "exists", SqlTokenType.Exists );
        On = RegKeyword( "on", SqlTokenType.On );
        To = RegKeyword( "to", SqlTokenType.To );
        Of = RegKeyword( "of", SqlTokenType.Of );
        Top = RegKeyword( "top", SqlTokenType.Top );
        Escape = RegKeyword( "escape", SqlTokenType.Escape );
        Into = RegKeyword( "into", SqlTokenType.Into );
        From = RegKeyword( "from", SqlTokenType.From );
        Where = RegKeyword( "where", SqlTokenType.Where );
        Group = RegKeyword( "group", SqlTokenType.Group );
        Option = RegKeyword( "option", SqlTokenType.Option );
        Add = RegKeyword( "add", SqlTokenType.Add );
        Database = RegKeyword( "database", SqlTokenType.Database );
        External = RegKeyword( "external", SqlTokenType.External );
        Over = RegKeyword( "over", SqlTokenType.Over );
        Cross = RegKeyword( "cross", SqlTokenType.Cross );
        Foreign = RegKeyword( "foreign", SqlTokenType.Foreign );
        Clustered = RegKeyword( "clustered", SqlTokenType.Clustered );
        Left = RegKeyword( "left", SqlTokenType.Left );
        Percent = RegKeyword( "percent", SqlTokenType.Percent );
        Values = RegKeyword( "values", SqlTokenType.Values );
        Distinct = RegKeyword( "distinct", SqlTokenType.Distinct );
        Pivot = RegKeyword( "pivot", SqlTokenType.Pivot );
        Having = RegKeyword( "having", SqlTokenType.Having );
        Cursor = RegKeyword( "cursor", SqlTokenType.Cursor );
        Read = RegKeyword( "read", SqlTokenType.Read );
        Browse = RegKeyword( "browse", SqlTokenType.Browse );
        Collate = RegKeyword( "collate", SqlTokenType.Collate );
        OpenDataSource = RegKeyword( "opendatasource", SqlTokenType.OpenDataSource );
        OpenRowSet = RegKeyword( "openrowset", SqlTokenType.OpenRowSet );
        OpenXml = RegKeyword( "openxml", SqlTokenType.OpenXml );
        OpenQuery = RegKeyword( "openquery", SqlTokenType.OpenQuery );
        Default = RegKeyword( "default", SqlTokenType.Default );
        User = RegKeyword( "user", SqlTokenType.User );
        Current = RegKeyword( "current", SqlTokenType.Current );
        Varying = RegKeyword( "varying", SqlTokenType.Varying );

        FreeText = RegKeyword( "freetext", SqlTokenType.FreeText );
        FreeTextTable = RegKeyword( "freetexttable", SqlTokenType.FreeTextTable );
        Reconfigure = RegKeyword( "reconfigure", SqlTokenType.Reconfigure );
        References = RegKeyword( "references", SqlTokenType.References );
        Full = RegKeyword( "full", SqlTokenType.Full );
        Replication = RegKeyword( "replication", SqlTokenType.Replication );
        Bulk = RegKeyword( "bulk", SqlTokenType.Bulk );
        Check = RegKeyword( "check", SqlTokenType.Check );
        HoldLock = RegKeyword( "holdlock", SqlTokenType.HoldLock );
        Right = RegKeyword( "right", SqlTokenType.Right );
        Checkpoint = RegKeyword( "checkpoint", SqlTokenType.Checkpoint );
        Identity = RegKeyword( "identity", SqlTokenType.Identity );
        IdentityInsert = RegKeyword( "identity_insert", SqlTokenType.IdentityInsert );
        RowCount = RegKeyword( "rowcount", SqlTokenType.RowCount );
        Save = RegKeyword( "save", SqlTokenType.Save );
        Column = RegKeyword( "column", SqlTokenType.Column );
        Index = RegKeyword( "index", SqlTokenType.Index );
        Schema = RegKeyword( "schema", SqlTokenType.Schema );
        Inner = RegKeyword( "inner", SqlTokenType.Inner );
        SecurityAudit = RegKeyword( "securityaudit", SqlTokenType.SecurityAudit );
        Compute = RegKeyword( "compute", SqlTokenType.Compute );
        Constraint = RegKeyword( "constraint", SqlTokenType.Constraint );
        SessionUser = RegKeyword( "session_user", SqlTokenType.SessionUser );
        Within = RegKeyword( "within", SqlTokenType.Within );
        Preceding = RegKeyword( "preceding", SqlTokenType.Preceding );
        Unbounded = RegKeyword( "unbounded", SqlTokenType.Unbounded );
        Following = RegKeyword( "following", SqlTokenType.Following );
        Partition = RegKeyword( "partition", SqlTokenType.Partition );
        SetUser = RegKeyword( "setuser", SqlTokenType.SetUser );
        Join = RegKeyword( "join", SqlTokenType.Join );
        Shutdown = RegKeyword( "shutdown", SqlTokenType.Shutdown );
        Convert = RegKeyword( "convert", SqlTokenType.Convert );
        Key = RegKeyword( "key", SqlTokenType.Key );
        Statistics = RegKeyword( "statistics", SqlTokenType.Statistics );
        SystemUser = RegKeyword( "system_user", SqlTokenType.SystemUser );
        CurrentDate = RegKeyword( "current_date", SqlTokenType.CurrentDate );
        LineNo = RegKeyword( "lineno", SqlTokenType.LineNo );
        TableSample = RegKeyword( "tablesample", SqlTokenType.TableSample );
        CurrentTime = RegKeyword( "current_time", SqlTokenType.CurrentTime );
        Load = RegKeyword( "load", SqlTokenType.Load );
        TextSize = RegKeyword( "textsize", SqlTokenType.TextSize );
        CurrentTimestamp = RegKeyword( "current_timestamp", SqlTokenType.CurrentTimestamp );
        CurrentUser = RegKeyword( "current_user", SqlTokenType.CurrentUser );
        National = RegKeyword( "national", SqlTokenType.National );
        NullIf = RegKeyword( "nullif", SqlTokenType.NullIf );
        TSEqual = RegKeyword( "tsequal", SqlTokenType.TSEqual );
        Off = RegKeyword( "off", SqlTokenType.Off );
        Offsets = RegKeyword( "offsets", SqlTokenType.Offsets );
        Unique = RegKeyword( "unique", SqlTokenType.Unique );
        Unpivot = RegKeyword( "unpivot", SqlTokenType.Unpivot );
        Disk = RegKeyword( "disk", SqlTokenType.Disk );
        Dump = RegKeyword( "dump", SqlTokenType.Dump );
        ErrLvl = RegKeyword( "errlvl", SqlTokenType.ErrLvl );
        Restrict = RegKeyword( "restrict", SqlTokenType.Restrict );
        Cascade = RegKeyword( "cascade", SqlTokenType.Cascade );
        Revert = RegKeyword( "revert", SqlTokenType.Revert );
        Revoke = RegKeyword( "revoke", SqlTokenType.Revoke );
        Any = RegKeyword( "any", SqlTokenType.Any );
        Some = RegKeyword( "some", SqlTokenType.Some );
        Precision = RegKeyword( "precision", SqlTokenType.Precision );
        Exit = RegKeyword( "exit", SqlTokenType.Exit );
        Primary = RegKeyword( "primary", SqlTokenType.Primary );
        Plan = RegKeyword( "plan", SqlTokenType.Plan );
        File = RegKeyword( "file", SqlTokenType.File );
        FillFactor = RegKeyword( "fillfactor", SqlTokenType.FillFactor );
        Public = RegKeyword( "public", SqlTokenType.Public );
        Authorization = RegKeyword( "authorization", SqlTokenType.Authorization );
        Distributed = RegKeyword( "distributed", SqlTokenType.Distributed );
        Coalesce = RegKeyword( "coalesce", SqlTokenType.Coalesce );
        Rule = RegKeyword( "rule", SqlTokenType.Rule );
        IdentityCol = RegKeyword( "identitycol", SqlTokenType.IdentityCol );
        RowguidCol = RegKeyword( "rowguidcol", SqlTokenType.RowguidCol );
        Contains = RegKeyword( "contains", SqlTokenType.Contains );
        ContainsTable = RegKeyword( "containstable", SqlTokenType.ContainsTable );
        NoCheck = RegKeyword( "nocheck", SqlTokenType.NoCheck );
        NonClustered = RegKeyword( "nonclustered", SqlTokenType.NonClustered );
        Double = RegKeyword( "double", SqlTokenType.Double );
        Outer = RegKeyword( "outer", SqlTokenType.Outer );

        // SqlTokenType.IdentifierReservedStatement values.
        Select = RegKeyword( "select", SqlTokenType.Select );
        Begin = RegKeyword( "begin", SqlTokenType.Begin );
        End = RegKeyword( "end", SqlTokenType.End );
        Create = RegKeyword( "create", SqlTokenType.Create );
        Drop = RegKeyword( "drop", SqlTokenType.Drop );
        Alter = RegKeyword( "alter", SqlTokenType.Alter );
        Declare = RegKeyword( "declare", SqlTokenType.Declare );
        Break = RegKeyword( "break", SqlTokenType.Break );
        Continue = RegKeyword( "continue", SqlTokenType.Continue );
        Goto = RegKeyword( "goto", SqlTokenType.Goto );
        While = RegKeyword( "while", SqlTokenType.While );
        If = RegKeyword( "if", SqlTokenType.If );
        Deallocate = RegKeyword( "deallocate", SqlTokenType.Deallocate );
        Close = RegKeyword( "close", SqlTokenType.Close );
        Fetch = RegKeyword( "fetch", SqlTokenType.Fetch );
        Open = RegKeyword( "open", SqlTokenType.Open );
        Return = RegKeyword( "return", SqlTokenType.Return );
        Execute = RegKeyword( "execute", SqlTokenType.Execute );
        _keywords.Add( "exec", SqlTokenType.Execute );
        Set = RegKeyword( "set", SqlTokenType.Set );
        Update = RegKeyword( "update", SqlTokenType.Update );
        Insert = RegKeyword( "insert", SqlTokenType.Insert );
        Raiserror = RegKeyword( "raiserror", SqlTokenType.Raiserror );
        WaitFor = RegKeyword( "waitfor", SqlTokenType.WaitFor );
        Use = RegKeyword( "use", SqlTokenType.Use );
        Truncate = RegKeyword( "truncate", SqlTokenType.Truncate );
        Print = RegKeyword( "print", SqlTokenType.Print );
        Commit = RegKeyword( "commit", SqlTokenType.Commit );
        Rollback = RegKeyword( "rollback", SqlTokenType.Rollback );
        Delete = RegKeyword( "delete", SqlTokenType.Delete );
        Updatetext = RegKeyword( "updatetext", SqlTokenType.Updatetext );
        Merge = RegKeyword( "merge", SqlTokenType.Merge );
        Kill = RegKeyword( "kill", SqlTokenType.Kill );
        Readtext = RegKeyword( "readtext", SqlTokenType.Readtext );
        Writetext = RegKeyword( "writetext", SqlTokenType.Writetext );
        Dbcc = RegKeyword( "dbcc", SqlTokenType.Dbcc );
        Go = RegKeyword( "go", SqlTokenType.Go );
        Backup = RegKeyword( "backup", SqlTokenType.Backup );
        Restore = RegKeyword( "restore", SqlTokenType.Restore );
        Grant = RegKeyword( "grant", SqlTokenType.Grant );
        Deny = RegKeyword( "deny", SqlTokenType.Deny );

        Transformer = RegKeyword( "transformer", SqlTokenType.Transformer );
        Parameter = RegKeyword( "parameter", SqlTokenType.Parameter );
        After = RegKeyword( "after", SqlTokenType.After );
        Before = RegKeyword( "before", SqlTokenType.Before );
        Around = RegKeyword( "around", SqlTokenType.Around );
        Inject = RegKeyword( "inject", SqlTokenType.Inject );
        Single = RegKeyword( "single", SqlTokenType.Single );
        Each = RegKeyword( "each", SqlTokenType.Each );
        Replace = RegKeyword( "replace", SqlTokenType.Replace );
        Scope = RegKeyword( "scope", SqlTokenType.Scope );
        //Largest = RegKeyword( "largest", SqlTokenType.Largest );
        //Deepest = RegKeyword( "deepest", SqlTokenType.Deepest );
        //Nodes = RegKeyword( "nodes", SqlTokenType.Nodes );
        Part = RegKeyword( "part", SqlTokenType.Part );
        Statement = RegKeyword( "statement", SqlTokenType.Statement );
        Range = RegKeyword( "range", SqlTokenType.Range );
        Combine = RegKeyword( "combine", SqlTokenType.Combine );

        Debug.Assert( (int)SqlTokenType.AssignOperatorCount == 9 );
        Assign = RegTerminal( SqlTokenType.Assign, "=" );
        BitwiseOrAssign = RegTerminal( SqlTokenType.BitwiseOrAssign, "|=" );
        BitwiseAndAssign = RegTerminal( SqlTokenType.BitwiseAndAssign, "&=" );
        BitwiseXOrAssign = RegTerminal( SqlTokenType.BitwiseXOrAssign, "^=" );
        PlusAssign = RegTerminal( SqlTokenType.PlusAssign, "+=" );
        MinusAssign = RegTerminal( SqlTokenType.MinusAssign, "-=" );
        DivideAssign = RegTerminal( SqlTokenType.DivideAssign, "/=" );
        MultAssign = RegTerminal( SqlTokenType.MultAssign, "*=" );
        ModuloAssign = RegTerminal( SqlTokenType.ModuloAssign, "%=" );

        Debug.Assert( (int)SqlTokenType.BasicOperatorCount == 9 );
        BitwiseOr = RegTerminal( SqlTokenType.BitwiseOr, "|" );
        BitwiseXOr = RegTerminal( SqlTokenType.BitwiseXOr, "^" );
        BitwiseAnd = RegTerminal( SqlTokenType.BitwiseAnd, "&" );
        Plus = RegTerminal( SqlTokenType.Plus, "+" );
        Minus = RegTerminal( SqlTokenType.Minus, "-" );
        Mult = RegTerminal( SqlTokenType.Mult, "*" );
        Divide = RegTerminal( SqlTokenType.Divide, "/" );
        Modulo = RegTerminal( SqlTokenType.Modulo, "%" );
        BitwiseNot = RegTerminal( SqlTokenType.BitwiseNot, "~" );

        Debug.Assert( (int)SqlTokenType.CompareOperatorCount == 9 );
        Equal = RegTerminal( SqlTokenType.Equal, "=" );
        Greater = RegTerminal( SqlTokenType.Greater, ">" );
        Less = RegTerminal( SqlTokenType.Less, "<" );
        GreaterOrEqual = RegTerminal( SqlTokenType.GreaterOrEqual, ">=" );
        LessOrEqual = RegTerminal( SqlTokenType.LessOrEqual, "<=" );
        NotEqualTo = RegTerminal( SqlTokenType.NotEqualTo, "<>" );
        Different = RegTerminal( SqlTokenType.Different, "!=" );
        NotGreaterThan = RegTerminal( SqlTokenType.NotGreaterThan, "!>" );
        NotLessThan = RegTerminal( SqlTokenType.NotLessThan, "!<" );

        Debug.Assert( (int)SqlTokenType.PunctuationCount == 11 );
        Dot = (SqlTokenDot)RegTerminal( SqlTokenType.Dot, "." );
        Comma = (SqlTokenComma)RegTerminal( SqlTokenType.Comma, "," );
        SemiColon = RegTerminal( SqlTokenType.SemiColon, ";" );
        Colon = RegTerminal( SqlTokenType.Colon, ":" );
        DoubleColon = (SqlTokenDoubleColon)RegTerminal( SqlTokenType.DoubleColons, "::" );
        QuestionMark = RegTerminal( SqlTokenType.QuestionMark, "?" );
        DoubleQuestionMark = RegTerminal( SqlTokenType.DoubleQuestionMark, "??" );
        TripleQuestionMark = RegTerminal( SqlTokenType.TripleQuestionMark, "???" );
        QuadrupleQuestionMark = RegTerminal( SqlTokenType.QuadrupleQuestionMark, "????" );
        DoubleDots = (SqlTokenMultiDots)RegTerminal( SqlTokenType.DoubleDots, ".." );
        TripleDots = (SqlTokenMultiDots)RegTerminal( SqlTokenType.TripleDots, "..." );

        CommaOneSpace = new SqlTokenComma( null, SqlTrivia.OneSpace );

        OpenPar = (SqlTokenOpenPar)RegTerminal( SqlTokenType.OpenPar, "(" );
        ClosePar = (SqlTokenClosePar)RegTerminal( SqlTokenType.ClosePar, ")" );
        OpenBracket = RegTerminal( SqlTokenType.OpenBracket, "[" );
        CloseBracket = RegTerminal( SqlTokenType.CloseBracket, "]" );
        OpenCurly = RegTerminal( SqlTokenType.OpenCurly, "{" );
        CloseCurly = RegTerminal( SqlTokenType.CloseCurly, "}" );
        OpenCurlyInCurly = RegTerminal( SqlTokenType.OpenCurlyInCurly, "{{" );
        CloseCurlyInCurly = RegTerminal( SqlTokenType.CloseCurlyInCurly, "}}" );

        _typeToString.Add( SqlTokenType.IdentifierStar, "*" );
        IdentifierStar = new SqlTokenIdentifier( SqlTokenType.IdentifierStar, "*", null, null );

        _typeToString.Add( SqlTokenType.None, "¤None" );
        _typeToString.Add( SqlTokenType.ErrorMask, "¤Error" );
        _typeToString.Add( SqlTokenType.EndOfInput, "¤EndOfInput" );
        _typeToString.Add( SqlTokenType.BegOfInput, "¤BegOfInput" );
        _typeToString.Add( SqlTokenType.ErrorInvalidChar, "¤ErrorInvalidChar" );
        _typeToString.Add( SqlTokenType.ErrorStringUnterminated, "¤ErrorStringUnterminated" );
        _typeToString.Add( SqlTokenType.ErrorIdentifierUnterminated, "¤ErrorIdentifierUnterminated" );
        _typeToString.Add( SqlTokenType.ErrorNumberUnterminatedValue, "¤ErrorNumberUnterminatedValue" );
        _typeToString.Add( SqlTokenType.ErrorNumberValue, "¤ErrorNumberValue" );
        _typeToString.Add( SqlTokenType.ErrorNumberIdentifierStartsImmediately, "¤ErrorNumberIdentifierStartsImmediately" );

        BegOfInput = new SqlTokenError( SqlTokenTypeError.BegOfInput );
        EndOfInput = new SqlTokenError( SqlTokenTypeError.EndOfInput );

        _typeToString.Add( SqlTokenType.String, "¤String" );
        _typeToString.Add( SqlTokenType.UnicodeString, "¤UnicodeString" );
        _typeToString.Add( SqlTokenType.Integer, "¤Integer" );
        _typeToString.Add( SqlTokenType.Binary, "¤Binary" );
        _typeToString.Add( SqlTokenType.Float, "¤Float" );
        _typeToString.Add( SqlTokenType.Decimal, "¤Decimal" );
        _typeToString.Add( SqlTokenType.Money, "¤Money" );
        _typeToString.Add( SqlTokenType.StarComment, "¤StarComment" );
        _typeToString.Add( SqlTokenType.LineComment, "¤LineComment" );

        _typeToString.Add( SqlTokenType.IdentifierStandard, "¤IdentifierStandard" );
        _typeToString.Add( SqlTokenType.IdentifierQuoted, "¤IdentifierQuoted" );
        _typeToString.Add( SqlTokenType.IdentifierQuotedBracket, "¤IdentifierQuotedBracket" );
        _typeToString.Add( SqlTokenType.IdentifierVariable, "¤IdentifierVariable" );
        _typeToString.Add( SqlTokenType.IdentifierSpecial, "¤IdentifierSpecial" );
    }


    [Conditional( "DEBUG" )]
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
