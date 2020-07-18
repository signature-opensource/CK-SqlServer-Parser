using System;
using System.Data;

namespace CK.SqlServer.Parser
{
    /// <summary>
    /// Tokens definition.
    /// </summary>
    /// <remarks>
    /// There are only 8 operator precedence levels in T-SQL (http://msdn.microsoft.com/en-us/library/ms190276.aspx).
    /// 
    /// Operator                                                            Description
    ///
    /// 15       (                                                          Expression grouping & function parameters.
    /// 14       .                                                          Dotted names & method calls.
    /// 13       ~                                                          Bitwise NOT and collate.
    /// 12       * /  %                                                     Multiplication, division, modulo division.
    /// 11       + - &amp; ^ |                                              + (for "Positive", "Add" and "Concatenate"), - (for "Negative" and "Subtract"), Bitwise AND, Bitwise Exclusive OR, and Bitwise OR.
    /// 
    /// 10       =(1) &gt; &lt; &gt;= &lt;= &lt;&gt; != !&gt; !&lt;         Comparison operators (last 3 ones are not ISO). 
    ///          IS BETWEEN LIKE IN                                         I added the IS keyword to handle IS [NOT] NULL and moved BETWEEN, IN and LIKE to this level and category.
    ///          
    /// 9        NOT                                                        Logical NOT (NOT as a LED introduces Between and Like. NOT has a 10 -strongest- left binding power).
    /// 8        AND                                                        Logical AND.
    /// 
    /// 7        OR                                                         Logical OR.
    ///          (moved to "comparison": IN, BETWEEN and LIKE)              Note: BETWEEN, IN and LIKE are here in the doc. 
    ///                                                                     It's not the right level up to me since the they can be followed by expressions and then by AND or OR operators.
    ///                                                                     Such AND or OR operators must not take precedence on the LIKE/IN/BETWEEN operator.
    ///                                                                     Actually, only LIKE MUST be upgraded to "comparison" operator because:
    ///                                                                     - BETWEEN can use an explicit right binding power of "comparison level" for its Start and Stop elements.
    ///                                                                     - IN, thanks to its required parenthesis can not "eat" the AND/OR following tokens.
    ///                                                                     But, for the sake of coherency, IN and BETWEEN are considered just like LIKE.
    ///          (Set operators are considered as identifiers                                                                    
    ///          for KoCall: ALL, ANY, EXISTS, SOME)                        All these operators are "like" function call i.e.: exist(...) or any(...).
    ///                                                                     Note: Exists is enclosable (like other KoCall) whereas any, some and all are not enclosable.
    /// 
    /// 6        = += -= *= /= %= &amp;= |= ^=                              Assignments (IsAssignOperator).
    ///        
    /// 5       Intersect                                                   Intersect, union and except have the same level as comma in msdn (it is not true: intersect &gt; except &gt; union [all]).                     
    /// 4       Except                                                      They act as binary operators between "Select Specification".                  
    /// 3       Union                                                                        
    /// 2       Order, For                                                  Consider them as operators (where left side is ISelectSpecification).
    /// 1       ,                                                           List separator (comma).
    /// 
    /// (1) For '=' token, disambiguation between Comparison and Assignment requires a context hint: we need to know if we are in a "assignment context" or not.
    ///     This must be done at a higher level than in <see cref="SqlTokenizer"/>.
    /// 
    /// </remarks>
    [Flags]
    public enum SqlTokenType
    {
        /// <summary>
        /// Not a token per-se.
        /// Used to denote white space.
        /// </summary>
        None = 0,

        #region SqlTokenTypeError values bits (negative values)
        /// <summary>
        /// Any negative value indicates an error or the end or beg of the input.
        /// </summary>
        IsErrorOrEndOfInput = SqlTokenTypeError.IsErrorOrEndOfInput,
        /// <summary>
        /// Same value as <see cref="SqlTokenTypeError.EndOfInput"/>.
        /// </summary>
        EndOfInput = SqlTokenTypeError.EndOfInput,
        /// <summary>
        /// Same value as <see cref="SqlTokenTypeError.BegOfInput"/>.
        /// This is not used as the parser level (the tokenizer always positions itself on the first token
        /// or the end of the inpit). This type is available as a helper to other layers.
        /// </summary>
        BegOfInput = SqlTokenTypeError.BegOfInput,

        IsError = SqlTokenTypeError.IsError,
        ErrorMask = SqlTokenTypeError.ErrorMask,
        ErrorTokenizerMask = SqlTokenTypeError.ErrorTokenizerMask,

        ErrorInvalidChar = SqlTokenTypeError.ErrorInvalidChar,
        ErrorIdentifierUnterminated = SqlTokenTypeError.ErrorIdentifierUnterminated,
        ErrorStringUnterminated = SqlTokenTypeError.ErrorStringUnterminated,
        ErrorNumberUnterminatedValue = SqlTokenTypeError.ErrorNumberUnterminatedValue,
        ErrorNumberValue = SqlTokenTypeError.ErrorNumberValue,
        ErrorNumberIdentifierStartsImmediately = SqlTokenTypeError.ErrorNumberIdentifierStartsImmediately,
        ErrorMustDoubleOpenCurly = SqlTokenTypeError.ErrorMustDoubleOpenCurly,
        #endregion

        #region Operator precedence bits n°28 to 24 (5 bits - 32 levels - actual levels are between 0 to 15, bit n°28 is unused).
        OpLevelShift = 24,
        OpLevelMask = 15 << OpLevelShift,

        OpLevel00 = 0,
        OpLevel01 = 1 << OpLevelShift,
        OpLevel02 = 2 << OpLevelShift,
        OpLevel03 = 3 << OpLevelShift,
        OpLevel04 = 4 << OpLevelShift,
        OpLevel05 = 5 << OpLevelShift,
        OpLevel06 = 6 << OpLevelShift,
        OpLevel07 = 7 << OpLevelShift, 
        OpLevel08 = 8 << OpLevelShift,
        OpLevel09 = 9 << OpLevelShift,
        OpLevel10 = 10 << OpLevelShift, OpComparisonLevel = OpLevel10, OpNotRightLevel = OpLevel10,
        OpLevel11 = 11 << OpLevelShift,
        OpLevel12 = 12 << OpLevelShift,
        OpLevel13 = 13 << OpLevelShift,
        OpLevel14 = 14 << OpLevelShift,
        OpLevel15 = 15 << OpLevelShift,
        #endregion

        /// <summary>
        /// Combines all IsXXXXOperator (Assign, Basic, Compare).
        /// </summary>
        AllOperatorMask = IsAssignOperator | IsBasicOperator | IsCompareOperator,

        /// <summary>
        /// Mask that covers operators, punctuations and brackets: the token is fully defined by 
        /// the <see cref="SqlTokenType"/> itself (no associated value is necessary).
        /// </summary>
        TerminalMask = AllOperatorMask | IsBracket | IsPunctuation,

        /// <summary>
        /// Mask that covers literals: IsString &amp; IsNumber.
        /// </summary>
        LitteralMask = IsString | IsNumber,

        /// <summary>
        /// Mask that covers IsXXX discriminators (including <see cref="IsComment"/>).
        /// </summary>
        TokenDiscriminatorMask = IsAssignOperator
                                    | IsBasicOperator
                                    | IsBracket
                                    | IsCompareOperator
                                    | IsIdentifier
                                    | IsNumber
                                    | IsPunctuation
                                    | IsString
                                    | IsComment,

        #region Token discriminators bits n°23 to 15 (IsAssignOperator to IsComment).
        /// <summary>
        /// Covers = |= &amp;= ^= += -= /= *= and %=.
        /// </summary>
        IsAssignOperator = 1 << 23,

        /// <summary>
        /// Covers binary operators |, ^, &amp;, +, -, /, *, % and the unary ~ (bitwise not).
        /// </summary>
        IsBasicOperator = 1 << 22,

        /// <summary>
        /// Covers [], () and {}.
        /// </summary>
        IsBracket = 1 << 21,

        /// <summary>
        /// Covers = &gt; &lt; &gt;= &lt;= &lt;&gt; != !&gt; and !&lt;.
        /// </summary>
        IsCompareOperator = 1 << 20,

        /// <summary>
        /// Covers identifiers.
        /// </summary>
        IsIdentifier = 1 << 19,

        /// <summary>
        /// Covers binary, money, float and integer (hexadecimal). 
        /// </summary>
        IsNumber = 1 << 18,

        /// <summary>
        /// Covers dot ".", comma "," and semicolon ";".
        /// </summary>
        IsPunctuation = 1 << 17,

        /// <summary>
        /// Covers strings ('string' or N'string').
        /// </summary>
        IsString = 1 << 16,

        /// <summary>
        /// Covers /* ... */ block as well as -- line comment.
        /// </summary>
        IsComment = 1 << 15,

        #endregion

        /// <summary>
        /// Masks that covers all discriminators except IsIdentifier, IsNumber and IsString and IsErrorOrEndOfInput.
        /// This is a "raw" mask because it does not cover quoted identifiers and ansi strings nor N'unicode strings'
        /// (the latter requires a separator on its left when following an identifier or a number, but none on its right
        /// except if it is an 'ansi' string).
        /// Note that even if two subsquent strings are not valid in a sql script, we handle them correctly:
        /// a 'ansi'N'unicode' is valid whereas 'ansi''ansi' and N'unicode''ansi' require a separator.
        /// <see cref="SqlToken.RequiresSeparatorBetween(SqlTokenType, SqlTokenType)"/> handles this.
        /// </summary>
        RawActualSeparatorMask = IsErrorOrEndOfInput | IsAssignOperator | IsBasicOperator | IsBracket | IsCompareOperator | IsPunctuation | IsComment,

        AssignOperatorCount = 9,
        #region IsAssignOperator: =, |=, &=, ^=, +=, -=, /=, *= and %=.
        /// <summary>
        /// Single equal character (=).
        /// </summary>
        Assign = IsAssignOperator | OpLevel06 | 1,

        /// <summary>
        /// Bitwise Or assignment (|=).
        /// </summary>
        BitwiseOrAssign = IsAssignOperator | OpLevel06 | 2,

        /// <summary>
        /// Bitwise And assignment (&amp;=).
        /// </summary>
        BitwiseAndAssign = IsAssignOperator | OpLevel06 | 3,

        /// <summary>
        /// Xor binary (^) operator assignment (^=).
        /// </summary>
        BitwiseXOrAssign = IsAssignOperator | OpLevel06 | 4,

        /// <summary>
        /// Add assignment (+=).
        /// </summary>
        PlusAssign = IsAssignOperator | OpLevel06 | 5,

        /// <summary>
        /// Substract assignment (-=).
        /// </summary>
        MinusAssign = IsAssignOperator | OpLevel06 | 6,

        /// <summary>
        /// Divide assignment (/=).
        /// </summary>
        DivideAssign = IsAssignOperator | OpLevel06 | 7,

        /// <summary>
        /// Multiplication assignment (*=).
        /// </summary>
        MultAssign = IsAssignOperator | OpLevel06 | 8,

        /// <summary>
        /// Modulo assignment (%=).
        /// </summary>
        ModuloAssign = IsAssignOperator | OpLevel06 | AssignOperatorCount,

        #endregion

        BasicOperatorCount = 9,
        #region IsBasicOperator: |, ^, &, +, -, /, *, % and the unary ~.

        /// <summary>
        /// Single pipe (|) bitwise OR operator.
        /// </summary>
        BitwiseOr = IsBasicOperator | OpLevel11 | 1,

        /// <summary>
        /// Xor binary (^) operator.
        /// </summary>
        BitwiseXOr = IsBasicOperator | OpLevel11 | 2,

        /// <summary>
        /// Single ampersand (&amp;) binary And operator.
        /// </summary>
        BitwiseAnd = IsBasicOperator | OpLevel11 | 3,

        /// <summary>
        /// Plus operator.
        /// </summary>
        Plus = IsBasicOperator | OpLevel11 | 4,

        /// <summary>
        /// Minus operator.
        /// </summary>
        Minus = IsBasicOperator | OpLevel11 | 5,

        /// <summary>
        /// Mult operator.
        /// </summary>
        Mult = IsBasicOperator | OpLevel12 | 6,

        /// <summary>
        /// Divide operator.
        /// </summary>
        Divide = IsBasicOperator | OpLevel12 | 7,
        /// <summary>
        /// Modulo.
        /// </summary>
        Modulo = IsBasicOperator | OpLevel12 | 8,
        /// <summary>
        /// Biwise Not (~).
        /// </summary>
        BitwiseNot = IsBasicOperator | OpLevel13 | 9,

        #endregion

        CompareOperatorCount = 9,
        #region IsCompareOperator: =, >, <, >=, <=, <>, !=, !> and !<.
        /// <summary>
        /// = character.
        /// </summary>
        Equal = IsCompareOperator | OpLevel10 | 1,
        /// <summary>
        /// One single &gt; character.
        /// </summary>
        Greater = IsCompareOperator | OpLevel10 | 2,
        /// <summary>
        /// One single &lt; character.
        /// </summary>
        Less = IsCompareOperator | OpLevel10 | 3,
        /// <summary>
        /// Greater than or equal (&gt;)
        /// </summary>
        GreaterOrEqual = IsCompareOperator | OpLevel10 | 4,
        /// <summary>
        /// Less than or equal (&lt;=) 
        /// </summary>
        LessOrEqual = IsCompareOperator | OpLevel10 | 5,
        /// <summary>
        /// &lt;&gt; (Not Equal To)
        /// </summary>
        NotEqualTo = IsCompareOperator | OpLevel10 | 6,
        
        /// <summary>
        /// C-like difference operator !=.
        /// </summary>
        Different = IsCompareOperator | OpLevel10 | 7,
        /// <summary>
        /// !&gt; (Not Greater Than)
        /// </summary>
        NotGreaterThan = IsCompareOperator | OpLevel10 | 8,
        /// <summary>
        /// !lt; (Not Less Than)
        /// </summary>
        NotLessThan = IsCompareOperator | OpLevel10 | 9,
        #endregion
        
        PunctuationCount = 11,
        #region Punctuations
        /// <summary>
        /// One dot.
        /// </summary>
        Dot = IsPunctuation | OpLevel14 | 1,
        /// <summary>
        /// The comma (,) is an operator.
        /// </summary>
        Comma = IsPunctuation | OpLevel01 | 2,
        /// <summary>
        /// Statement terminator (;).
        /// </summary>
        SemiColon = IsPunctuation | 3,
        /// <summary>
        /// One single colon (:).
        /// </summary>
        Colon = IsPunctuation | 4,
        /// <summary>
        /// Two colons :: are used to call static CLR methods ang GRANT/DENY 
        /// statements: GRANT SELECT ON OBJECT::Person.Address TO Albert;
        /// </summary>
        DoubleColons = IsPunctuation | OpLevel14 | 5,
        /// <summary>
        /// Question mark (?) does not belong to the T-Sql terminals.
        /// </summary>
        QuestionMark = IsPunctuation | 6,
        /// <summary>
        /// Double Question mark (??) does not belong to the T-Sql terminals.
        /// </summary>
        DoubleQuestionMark = IsPunctuation | 7,
        /// <summary>
        /// Triple Question mark (???) does not belong to the T-Sql terminals.
        /// This is the non-greedy wildcard. This could have been'?*?' but then the greedy would be '?*' that 
        /// is shorter...
        /// </summary>
        TripleQuestionMark = IsPunctuation | 8,
        /// <summary>
        /// Quadruple Question mark (????) does not belong to the T-Sql terminals.
        /// This is greedy wildcard. Could have been '?*'... but as of now, keeping only ? seems clearer...
        /// </summary>
        QuadrupleQuestionMark = IsPunctuation | 9,
        /// <summary>
        /// The .. is used in dbname..object and this select the default schema of the current user.
        /// </summary>
        DoubleDots = IsPunctuation | OpLevel14 | 10,
        /// <summary>
        /// The .. is used in servername...object and this select the default database and schema of the user.
        /// </summary>
        TripleDots = IsPunctuation | OpLevel14 | 11,

        #endregion

        /// <summary>
        /// String literal like 'string'.
        /// </summary>
        String = IsString | 1,

        /// <summary>
        /// Unicode string literal like N'string'.
        /// </summary>
        UnicodeString = IsString | 2,

        /// <summary>
        /// Binary string constant like 0x12Ef or 0x69048AEFDD010E
        /// is a kind of Number.
        /// </summary>
        Binary = IsNumber | 1,

        /// <summary>
        /// Integer constants like 1894 or 2.
        /// </summary>
        /// <remarks>
        /// Bits are integer 0 and 1.
        /// </remarks>
        Integer = IsNumber | 2,

        /// <summary>
        /// Decimal literals like 1894.1204 or 2.0. 
        /// "Decimal" is the ISO name of Sql Server specific "numeric".
        /// </summary>
        Decimal = IsNumber | 3,
 
        /// <summary>
        /// Float and real literals like 101.5E5 or .5e-2.
        /// </summary>
        Float = IsNumber | 4,

        /// <summary>
        /// Money literals like $12 or $542023.14.
        /// </summary>
        Money = IsNumber | 5,

        #region Identifiers
        /// <summary>
        /// IdentifierTypeMask covers bits n°14 to 11 (16 possible types) and bit n°19 (<see cref="IsIdentifier"/>).
        /// </summary>
        IdentifierTypeMask = IsIdentifier | 15 << 11,
        
        /// <summary>
        /// IdentifierValueMask covers bits n°7 to 0.
        /// </summary>
        IdentifierValueMask = 0xFF,

        /// <summary>
        /// Not reserved keywords that can start a statement like “throw”, “get”, “move”, “receive”, etc.
        /// Identifiers that can start a statement are the lower ones. Among them there are the
        /// non reserved keyword (these ones) and the reserved ones <see cref="IdentifierReservedStatement"/>.
        /// </summary>
        IdentifierStandardStatement = IsIdentifier | 0,

        /// <summary>
        /// Reserved keywords that starts a statement: “select”, “create”, “declare", “set”, etc.
        /// </summary>
        IdentifierReservedStatement = IsIdentifier | 1 << 11,

        /// <summary>
        /// Any identifier like “max”, a table name, but not a reserved keyword like “when”, “select”, "cursor" or “else”
        /// nor an <see cref="IdentifierDbType"/>.
        /// </summary>
        IdentifierStandard = IsIdentifier | 2 << 11,
        
        /// <summary>
        /// Identifiers that are reserved keywords (like “identity_insert”, “clustered”, “rule”, “as”, etc.) but 
        /// cannot start a statement.
        /// </summary>
        IdentifierReserved = IsIdentifier | 3 << 11,

        /// <summary>
        /// Denotes a "quoted identifier".
        /// </summary>
        IdentifierQuoted = IsIdentifier | 4 << 11,
        
        /// <summary>
        /// Denotes a [Quoted identifier].
        /// </summary>
        IdentifierQuotedBracket = IsIdentifier | 5 << 11,
        
        /// <summary>
        /// Special identifiers like star (in “select t.* from t)”, $identity, $Partition, $action etc.
        /// </summary>
        IdentifierSpecial = IsIdentifier | 6 << 11,

        /// <summary>
        /// SqlDbType like int, smallint, datetime, xml, etc.
        /// 'Table' is both IdentifierDbType and IdentifierReserved.
        /// </summary>
        IdentifierDbType = IsIdentifier | 7 << 11,

        /// <summary>
        /// 'Table' is a reserved keyword mapped to <see cref="SqlDbType.Structured"/>.
        /// </summary>
        IdentifierReservedDbType = IsIdentifier | 8 << 11,

        /// <summary>
        /// Variable token like @myVariableName or @@SystemFunctions like @@RowCount or @@Error.
        /// </summary>
        IdentifierVariable = IsIdentifier | 9 << 11,

        #region IdentifierStandardStatement values
        Throw = IdentifierStandardStatement | 1,
        Move = IdentifierStandardStatement | 2,
        Get = IdentifierStandardStatement | 3,
        Receive = IdentifierStandardStatement | 4,
        Send = IdentifierStandardStatement | 5,
        #endregion

        #region IdentifierStandard values
        Try             = IdentifierStandard | 1,
        Catch,
        Dialog,
        Conversation,
        Returns,
        Max,
        Readonly,
        Out,
        Output,
        /// <summary>
        /// Rows is the official ISO identifier. Sql Server also accepts Row.
        /// </summary>
        Rows,            
        Offset,
        First,
        Next,
        Only,
        Cast,
        Insensitive,
        Scroll,
        Mark,
        Json,
        SystemTime,
        Ties,
        Value,
        Matched,
        Recompile,
        Result,
        Sets,
        Undefined,
        Login,
        At,
        Using,
        Global,
        // OpenDataSource, OpenRowSet, OpenXml and OpenQuery are reserved keywords.
        // OpenJSON is not a rserved keyword.
        // OpenJSON and OpenXML are both rowset functions that support WITH format specification.
        OpenJSON,
        Encryption,
        SchemaBinding,
        Input,
        Called,
        NativeCompilation,
        Server,
        Last,
        XmlNamespaces,
        Zone,

        // This is not sql :)
        // These identifiers support SqlTXXX (transformer) nodes.
        Transformer,
        Parameter,
        After,
        Before,
        Around,
        Inject,
        Replace,
        Scope,
        Single,
        Each,
        //Largest,
        //Deepest,
        //Nodes,
        Range,
        Part,
        Statement,
        Combine,

        #endregion

        #region IdentifierSpecial values
        /// <summary>
        /// Star (*) token considered as an identifier instead of <see cref="Mult"/>.
        /// This token type is not produced by <see cref="SqlTokenizer"/> (transforming the token
        /// requires more knowledge of the syntactic context).
        /// </summary>
        IdentifierStar = IdentifierSpecial | 1,

        #endregion

        #region IdentifierReserved values
        #region Logical operators: not, or, and (Keywords all, any - same as "some" - and exists are identifiers handled as KoCall).
        /// <summary>
        /// NOT operator.
        /// </summary>
        Not = OpLevel09 | IdentifierReserved | 1,
        /// <summary>
        /// Logical OR operator.
        /// </summary>
        Or = OpLevel07 | IdentifierReserved | 2,
        /// <summary>
        /// Logical AND operator.
        /// </summary>
        And = OpLevel08 | IdentifierReserved | 3,
        #endregion

        #region Select operators: Union, Except, Intersect, Order, For, Option and Collate.
        /// <summary>
        /// Union between select specification (lowest precedence).
        /// </summary>
        Union = OpLevel03 | IdentifierReserved | 4,
        /// <summary>
        /// Except between select specification.
        /// </summary>
        Except = OpLevel04 | IdentifierReserved | 5,
        /// <summary>
        /// Intersect between select specification (highest precedence).
        /// </summary>
        Intersect = OpLevel05 | IdentifierReserved | 6,
        /// <summary>
        /// Order By is considered as an operator.
        /// </summary>
        Order = OpLevel02 | IdentifierReserved | 7,
        /// <summary>
        /// For (xml, browse...) is considered as an operator.
        /// </summary>
        For = OpLevel02 | IdentifierReserved | 8,
        /// <summary>
        /// Option ( {queryhint} ) is considered as an operator.
        /// </summary>
        Option = OpLevel02 | IdentifierReserved | 9,
        /// <summary>
        /// Collate is an operator that has a high precedence (the same as bitwise ~).
        /// </summary>
        Collate = OpLevel13 | IdentifierReserved | 10,

        #endregion

        #region Between, Like, In, Is (act as comparison operators).
        /// <summary>
        /// BETWEEN operator.
        /// </summary>
        Between = OpLevel10 | IdentifierReserved | 11,
        /// <summary>
        /// LIKE operator.
        /// </summary>
        Like = OpLevel10 | IdentifierReserved | 12,
        /// <summary>
        /// IN operator.
        /// </summary>
        In = OpLevel10 | IdentifierReserved | 13,
        /// <summary>
        /// IS operator.
        /// </summary>
        Is = OpLevel10 | IdentifierReserved | 14,
        #endregion

        /// <summary>
        /// Internal marker for IdentifierReserved numbering.
        /// </summary>
        StartIdentifierReservedNonOperator = IdentifierReserved | 15,
        Case        = StartIdentifierReservedNonOperator + 0,
        Null        = StartIdentifierReservedNonOperator + 1,
        When        = StartIdentifierReservedNonOperator + 2,
        By          = StartIdentifierReservedNonOperator + 3,
        All         = StartIdentifierReservedNonOperator + 4,
        Then        = StartIdentifierReservedNonOperator + 5,
        Else        = StartIdentifierReservedNonOperator + 6,
        Transaction = StartIdentifierReservedNonOperator + 7,
        With        = StartIdentifierReservedNonOperator + 8,
        Procedure   = StartIdentifierReservedNonOperator + 9,
        Function    = StartIdentifierReservedNonOperator + 10,
        View        = StartIdentifierReservedNonOperator + 11,
        Trigger     = StartIdentifierReservedNonOperator + 13,
        As          = StartIdentifierReservedNonOperator + 14,
        Asc         = StartIdentifierReservedNonOperator + 15,
        Desc        = StartIdentifierReservedNonOperator + 16,
        Exists      = StartIdentifierReservedNonOperator + 17,
        On          = StartIdentifierReservedNonOperator + 18,
        To          = StartIdentifierReservedNonOperator + 19,
        Of          = StartIdentifierReservedNonOperator + 20,
        Top         = StartIdentifierReservedNonOperator + 21,
        Escape      = StartIdentifierReservedNonOperator + 22,
        Into        = StartIdentifierReservedNonOperator + 23,
        From        = StartIdentifierReservedNonOperator + 24,
        Where       = StartIdentifierReservedNonOperator + 25,
        Group       = StartIdentifierReservedNonOperator + 26,
        Add         = StartIdentifierReservedNonOperator + 27,
        Database    = StartIdentifierReservedNonOperator + 28,
        External    = StartIdentifierReservedNonOperator + 29,
        Over        = StartIdentifierReservedNonOperator + 30,
        Cross       = StartIdentifierReservedNonOperator + 31,
        Foreign     = StartIdentifierReservedNonOperator + 32,
        Clustered   = StartIdentifierReservedNonOperator + 33,
        Left        = StartIdentifierReservedNonOperator + 34,
        Percent     = StartIdentifierReservedNonOperator + 35,
        Values      = StartIdentifierReservedNonOperator + 36,
        Distinct    = StartIdentifierReservedNonOperator + 37,
        Pivot       = StartIdentifierReservedNonOperator + 38,
        Having      = StartIdentifierReservedNonOperator + 39,
        Cursor      = StartIdentifierReservedNonOperator + 40,
        Read        = StartIdentifierReservedNonOperator + 41,
        Browse      = StartIdentifierReservedNonOperator + 42,
        OpenDataSource = StartIdentifierReservedNonOperator + 43,
        // An OpenRowSet call is a kind of table, just like OpenQuery.
        OpenRowSet  = StartIdentifierReservedNonOperator + 44,
        // OpenQuey and OpenJSON (that is not a reserved keyword), support
        // WITH options. 
        OpenQuery   = StartIdentifierReservedNonOperator + 45,
        OpenXml     = StartIdentifierReservedNonOperator + 46,
        Default     = StartIdentifierReservedNonOperator + 47,
        User        = StartIdentifierReservedNonOperator + 48,
        Current     = StartIdentifierReservedNonOperator + 49,
        Varying     = StartIdentifierReservedNonOperator + 50,
        FreeText = StartIdentifierReservedNonOperator + 51,
        FreeTextTable = StartIdentifierReservedNonOperator + 52,
        Outer = StartIdentifierReservedNonOperator + 53,
        Double = StartIdentifierReservedNonOperator + 54,
        NonClustered = StartIdentifierReservedNonOperator + 55,
        NoCheck = StartIdentifierReservedNonOperator + 56,
        ContainsTable = StartIdentifierReservedNonOperator + 57,
        Contains = StartIdentifierReservedNonOperator + 58,
        RowguidCol = StartIdentifierReservedNonOperator + 59,
        IdentityCol = StartIdentifierReservedNonOperator + 60,
        Rule = StartIdentifierReservedNonOperator + 61,
        Distributed = StartIdentifierReservedNonOperator + 62,
        Coalesce = StartIdentifierReservedNonOperator + 63,
        Authorization = StartIdentifierReservedNonOperator + 64,
        Revoke = StartIdentifierReservedNonOperator + 65,
        Restrict = StartIdentifierReservedNonOperator + 66,
        Cascade = StartIdentifierReservedNonOperator + 67,
        Any = StartIdentifierReservedNonOperator + 68,
        Revert = StartIdentifierReservedNonOperator + 69,
        Some = StartIdentifierReservedNonOperator + 70,
        Precision = StartIdentifierReservedNonOperator + 71,
        Exit = StartIdentifierReservedNonOperator + 72,
        Primary = StartIdentifierReservedNonOperator + 73,
        Plan = StartIdentifierReservedNonOperator + 74,
        File = StartIdentifierReservedNonOperator + 75,
        FillFactor = StartIdentifierReservedNonOperator + 76,
        Public = StartIdentifierReservedNonOperator + 77,
        ErrLvl = StartIdentifierReservedNonOperator + 78,
        Dump = StartIdentifierReservedNonOperator + 79,
        Disk = StartIdentifierReservedNonOperator + 80,
        Unpivot = StartIdentifierReservedNonOperator + 81,
        Unique = StartIdentifierReservedNonOperator + 82,
        Offsets = StartIdentifierReservedNonOperator + 83,
        Off = StartIdentifierReservedNonOperator + 84,
        TSEqual = StartIdentifierReservedNonOperator + 85,
        NullIf = StartIdentifierReservedNonOperator + 86,
        National = StartIdentifierReservedNonOperator + 87,
        CurrentUser = StartIdentifierReservedNonOperator + 88,
        CurrentTimestamp = StartIdentifierReservedNonOperator + 89,
        TextSize = StartIdentifierReservedNonOperator + 90,
        Load = StartIdentifierReservedNonOperator + 91,
        CurrentTime = StartIdentifierReservedNonOperator + 92,
        TableSample = StartIdentifierReservedNonOperator + 93,
        LineNo = StartIdentifierReservedNonOperator + 94,
        CurrentDate = StartIdentifierReservedNonOperator + 95,
        SystemUser = StartIdentifierReservedNonOperator + 96,
        Key = StartIdentifierReservedNonOperator + 97,
        Statistics = StartIdentifierReservedNonOperator + 98,
        Convert = StartIdentifierReservedNonOperator + 99,
        Shutdown = StartIdentifierReservedNonOperator + 100,
        Join = StartIdentifierReservedNonOperator + 101,
        SetUser = StartIdentifierReservedNonOperator + 102,
        Constraint = StartIdentifierReservedNonOperator + 103,
        Compute = StartIdentifierReservedNonOperator + 104,
        Reconfigure = StartIdentifierReservedNonOperator + 105,
        References = StartIdentifierReservedNonOperator + 106,
        Full = StartIdentifierReservedNonOperator + 107,
        Replication = StartIdentifierReservedNonOperator + 108,
        Bulk = StartIdentifierReservedNonOperator + 109,
        Check = StartIdentifierReservedNonOperator + 110,
        HoldLock = StartIdentifierReservedNonOperator + 111,
        Right = StartIdentifierReservedNonOperator + 112,
        Checkpoint = StartIdentifierReservedNonOperator + 113,
        Identity = StartIdentifierReservedNonOperator + 114,
        IdentityInsert = StartIdentifierReservedNonOperator + 115,
        RowCount = StartIdentifierReservedNonOperator + 116,
        Save = StartIdentifierReservedNonOperator + 117,
        Column = StartIdentifierReservedNonOperator + 118,
        Index = StartIdentifierReservedNonOperator + 119,
        Schema = StartIdentifierReservedNonOperator + 120,
        Inner = StartIdentifierReservedNonOperator + 121,
        SecurityAudit = StartIdentifierReservedNonOperator + 122,
        SessionUser = StartIdentifierReservedNonOperator + 123,
        Within = StartIdentifierReservedNonOperator + 124,

        #endregion

        #region IdentifierReservedStatement values
        Select = IdentifierReservedStatement | 1,
        Begin       = IdentifierReservedStatement | 2,
        End         = IdentifierReservedStatement | 3,
        Create      = IdentifierReservedStatement | 4,
        Drop        = IdentifierReservedStatement | 5,
        Alter       = IdentifierReservedStatement | 6,
        Declare     = IdentifierReservedStatement | 7,
        Break       = IdentifierReservedStatement | 8,
        Continue    = IdentifierReservedStatement | 9,
        Goto        = IdentifierReservedStatement | 10,
        While       = IdentifierReservedStatement | 11,
        If          = IdentifierReservedStatement | 12,
        Deallocate  = IdentifierReservedStatement | 13,
        Close       = IdentifierReservedStatement | 14,
        Open        = IdentifierReservedStatement | 15,
        Fetch       = IdentifierReservedStatement | 16,
        Return      = IdentifierReservedStatement | 17,
        Execute     = IdentifierReservedStatement | 18,
        Set         = IdentifierReservedStatement | 19,
        Update      = IdentifierReservedStatement | 20,
        Insert      = IdentifierReservedStatement | 21,
        Raiserror   = IdentifierReservedStatement | 22,
        WaitFor     = IdentifierReservedStatement | 23,
        Use         = IdentifierReservedStatement | 24,
        Truncate    = IdentifierReservedStatement | 25,
        Print       = IdentifierReservedStatement | 26,
        Commit      = IdentifierReservedStatement | 27,
        Rollback    = IdentifierReservedStatement | 28,
        Delete      = IdentifierReservedStatement | 29,
        Updatetext  = IdentifierReservedStatement | 30,
        Merge       = IdentifierReservedStatement | 31,
        Kill        = IdentifierReservedStatement | 32,
        Readtext    = IdentifierReservedStatement | 33,
        Writetext   = IdentifierReservedStatement | 34,
        Dbcc        = IdentifierReservedStatement | 35,
        Go          = IdentifierReservedStatement | 36,
        Backup      = IdentifierReservedStatement | 37,
        Restore     = IdentifierReservedStatement | 38,
        Grant       = IdentifierReservedStatement | 39,
        Deny        = IdentifierReservedStatement | 40,

        #endregion

        #region IdentifierDbType values
        XmlDbType = IdentifierDbType | 0,
        DateTimeOffsetDbType = IdentifierDbType | 1,
        DateTime2DbType = IdentifierDbType | 2,
        DateTimeDbType = IdentifierDbType | 3,
        SmallDateTimeDbType = IdentifierDbType | 4,
        DateDbType = IdentifierDbType | 5,
        TimeDbType = IdentifierDbType | 6,
        FloatDbType = IdentifierDbType | 7,
        RealDbType = IdentifierDbType | 8,
        DecimalDbType = IdentifierDbType | 9,
        MoneyDbType = IdentifierDbType | 10,
        SmallMoneyDbType = IdentifierDbType | 11,
        BigIntDbType = IdentifierDbType | 12,
        IntDbType = IdentifierDbType | 13,
        SmallIntDbType = IdentifierDbType | 14,
        TinyIntDbType = IdentifierDbType | 15,
        BitDbType = IdentifierDbType | 16,
        NTextDbType = IdentifierDbType | 17,
        TextDbType = IdentifierDbType | 18,
        ImageDbType = IdentifierDbType | 19,
        TimestampDbType = IdentifierDbType | 20,
        UniqueIdentifierDbType = IdentifierDbType | 21,
        NVarCharDbType = IdentifierDbType | 22,
        NCharDbType = IdentifierDbType | 23,
        VarCharDbType = IdentifierDbType | 24,
        CharDbType = IdentifierDbType | 25,
        VarBinaryDbType = IdentifierDbType | 26,
        BinaryDbType = IdentifierDbType | 27,
        VariantDbType = IdentifierDbType | 28,
        TableDbType = IdentifierReservedDbType | 29,
        #endregion

        #endregion

        /// <summary>
        /// Star comment: /*...*/
        /// </summary>
        StarComment = IsComment | 1,

        /// <summary>
        /// Line comment: --... 
        /// </summary>
        LineComment = IsComment | 2,

        RoundBracket = IsBracket | 1,
        CurlyBracket = IsBracket | 4,
        OpenBracket = IsBracket | 8,
        CloseBracket = IsBracket | 16,

        OpenPar = RoundBracket | OpenBracket | OpLevel15,
        ClosePar = RoundBracket | CloseBracket,
        OpenCurly = CurlyBracket | OpenBracket,
        CloseCurly = CurlyBracket | CloseBracket,
        // These are { and } inside a top level { curly braces string }.
        // Open and close curly braces must be doubled inside a curly braces string.
        OpenCurlyInCurly = CurlyBracket | OpenBracket | 32,
        CloseCurlyInCurly = CurlyBracket | CloseBracket | 32,

    }

}
