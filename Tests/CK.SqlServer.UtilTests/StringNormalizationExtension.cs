using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CK.SqlServer.UtilTests
{
    public static class StringNormalizationExtension
    {
        public static string NormalizeEOL( this string @this )
        {
            if( Environment.NewLine == "\r\n" )
            {
                return ToCRLF( @this );
            }
            else if( Environment.NewLine == "\n" )
            {
                return ToLF( @this );
            }
            throw new NotSupportedException( "Unsupported Environment.NewLine." );
        }

        static readonly Regex _rLFOnly = new Regex( @"(?<!\r)\n", RegexOptions.CultureInvariant );

        static string ToCRLF( string text )
        {
            return _rLFOnly.Replace( text, "\r\n" );
        }

        static string ToLF( string text )
        {
            return text.Replace( "\r\n", "\n" );
        }


    }
}
