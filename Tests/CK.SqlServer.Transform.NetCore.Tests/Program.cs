using NUnitLite;
using System.Reflection;

namespace CK.SqlServer.Transform.NetCore.Tests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return new AutoRun( Assembly.GetEntryAssembly() ).Execute(args);
        }
    }
}
