using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.SqlServer.Parser.Tests
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
