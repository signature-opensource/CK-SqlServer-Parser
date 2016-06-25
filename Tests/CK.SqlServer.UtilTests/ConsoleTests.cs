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
            TestHelper.LogsToConsole = !TestHelper.LogsToConsole;
        }
    }
}
