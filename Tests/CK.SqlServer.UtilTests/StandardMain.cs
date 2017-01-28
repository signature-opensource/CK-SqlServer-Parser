#if !NET451
using NUnit.Common;
using NUnitLite;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CK.SqlServer.UtilTests
{
    public class StandardMain
    {
        public static int Main(Type programType, string[] args)
        {
            int idxPause = HandleArgument(ref args, "-pause");
            int result = new AutoRun(programType.GetTypeInfo().Assembly)
                .Execute(args, new ExtendedTextWrapper(Console.Out), Console.In);
            if (idxPause >= 0)
            {
                Console.Write("Hit a key.");
                Console.ReadKey();
            }
            return result;
        }

        private static int HandleArgument(ref string[] args, string argument)
        {
            int idxPause = Array.IndexOf(args, argument);
            if (idxPause >= 0)
            {
                var l = new List<string>(args);
                l.RemoveAt(idxPause);
                args = l.ToArray();
            }

            return idxPause;
        }
    }
}
#else
using System;
using System.Diagnostics;

namespace CK.SqlServer.UtilTests
{
    public class StandardMain
    {
        public static int Main(Type main, string[] args)
        {
            return 0;
        }

    }
}
#endif