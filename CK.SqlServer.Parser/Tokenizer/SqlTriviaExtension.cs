//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CK.SqlServer.Parser
//{
//    public static class SqlTriviaExtension
//    {
//        ///// <summary>
//        ///// Returns the trivias, either their actual content or only one space as long as there is at 
//        ///// least one non empty trivia.
//        ///// </summary>
//        ///// <param name="@this">This multiple trivias.</param>
//        ///// <param name="option">The option.</param>
//        ///// <returns>The string.</returns>
//        //public static string ToString( this IEnumerable<SqlTrivia> @this, SqlTriviaWriteOption option )
//        //{
//        //    if( option == SqlTriviaWriteOption.OneSpace )
//        //    {
//        //        return @this.Any( t => !t.IsEmpty ) ? " " : String.Empty;
//        //    }
//        //    return Write( @this, new StringBuilder(), option ).ToString();
//        //}

//    //    /// <summary>
//    //    /// Writes multiple trivias, either their actual content or only one space as long as there is at 
//    //    /// least one non empty trivia.
//    //    /// </summary>
//    //    /// <param name="@this">This multiple trivias.</param>
//    //    /// <param name="option">The option.</param>
//    //    /// <returns>The string.</returns>
//    //    public static StringBuilder Write( this IEnumerable<SqlTrivia> @this, StringBuilder b, SqlTriviaWriteOption option )
//    //    {
//    //        foreach( SqlTrivia t in @this )
//    //        {
//    //            if( t.Write( b, option ) && option == SqlTriviaWriteOption.OneSpace ) break;
//    //        }
//    //        return b;
//    //    }
//    //}
//}
