using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSmart.Unicode;

namespace UnicodeTests
{
    [TestClass]
    public class CodepointTests
    {
        [TestMethod]
        public void TestCodepointEquality()
        {
            Assert.AreEqual(new Codepoint('\n'), new Codepoint(10));
        }

        [TestMethod]
        public void TestCodepointComparison()
        {
            Assert.IsTrue(new Codepoint(13) > new Codepoint(10));
            Assert.IsFalse(new Codepoint(67) < new Codepoint(67));
        }
    }
}
