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
            Assume.That( MonitorTestHelper.TestHelper.IsExplicitAllowed, "Press Ctrl key to allow this test to run." );
            MonitorTestHelper.TestHelper.LogToConsole = !MonitorTestHelper.TestHelper.LogToConsole;
        }
    }
}
