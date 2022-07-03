using CK.Testing;
using NUnit.Framework;

namespace CK.SqlServer.UtilTests
{
    [TestFixture]
    public class ConsoleTests
    {
        [Test]
        [Explicit]
        public void toggle_console()
        {
            MonitorTestHelper.TestHelper.LogToConsole = !MonitorTestHelper.TestHelper.LogToConsole;
        }
    }
}
