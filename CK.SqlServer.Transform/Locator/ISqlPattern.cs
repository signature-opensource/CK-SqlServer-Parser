//using CK.SqlServer.Parser;
//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Collections;
//using CK.Core;

//namespace CK.SqlServer.Transform
//{
//    public enum SqlPatternStatus
//    {
//        None,
//        Included,
//        Excluded
//    }

//    public class SqlLocator
//    {

//    }



//    public static class SqlNodeLocationRangeExtensions
//    {
//        public static IEnumerable<SqlNodeLocationRange> Union( this IEnumerable<SqlNodeLocationRange> @this, IEnumerable<SqlNodeLocationRange> others )
//        {

//        }
//    }

//    public interface ISqlPattern
//    {
//        /// <summary>
//        /// Resets the state machine.
//        /// </summary>
//        void Reset();

//        /// <summary>
//        /// Accepts a new node path.
//        /// </summary>
//        SqlPatternStatus Accept( SqlNodeLocation node );

//    }
//}
