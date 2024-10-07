#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace CK.SqlServer.Parser;

public enum SqlTokenTypeError
{
    None = 0,

    /// <summary>
    /// Sign bit (bit n°31) is 1 to indicate an error or the end of the input.
    /// This allows easy and efficient error/end test: any negative token value marks the end.
    /// </summary>
    IsErrorOrEndOfInput = -2147483648,

    /// <summary>
    /// The end of input has only the most significant bit set.
    /// </summary>
    EndOfInput = IsErrorOrEndOfInput,

    /// <summary>
    /// The beginning of the input.
    /// </summary>
    BegOfInput = IsErrorOrEndOfInput | (1 << 30),

    /// <summary>
    /// Error bit (all kind of errors, but not the end of the input).
    /// </summary>
    IsError = 1 << 29,

    /// <summary>
    /// Error mask for any errors: all kind of errors have these 2 bits set.
    /// </summary>
    ErrorMask = IsErrorOrEndOfInput | IsError,

    /// <summary>
    /// Error mask for errors raised at the <see cref="SqlTokenizer"/> level: the 3 bits - n°28, 29 &amp; 31 - are set.
    /// </summary>
    ErrorTokenizerMask = IsErrorOrEndOfInput | IsError | (1 << 28),

    /// <summary>
    /// Invalid character.
    /// </summary>
    ErrorInvalidChar = ErrorTokenizerMask | 1,

    /// <summary>
    /// Whenever a non terminated string is encountered.
    /// </summary>
    ErrorStringUnterminated = ErrorTokenizerMask | 2,

    /// <summary>
    /// Whenever a non terminated identifier is encountered.
    /// </summary>
    ErrorIdentifierUnterminated = ErrorTokenizerMask | 3,

    /// <summary>
    /// Unterminated number.
    /// </summary>
    ErrorNumberUnterminatedValue = ErrorTokenizerMask | 4,

    /// <summary>
    /// Invalid number value.
    /// </summary>
    ErrorNumberValue = ErrorTokenizerMask | 5,

    /// <summary>
    /// Number value is immediately followed by an identifier: 45D for example.
    /// </summary>
    ErrorNumberIdentifierStartsImmediately = ErrorTokenizerMask | 6,

    /// <summary>
    /// A curly brace has not been doubled inside a curly braces string.
    /// </summary>
    ErrorMustDoubleOpenCurly = ErrorTokenizerMask | 7

}
