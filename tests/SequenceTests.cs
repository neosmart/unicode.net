using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSmart.Unicode;
using System.Linq;

namespace UnicodeTests
{
    [TestClass]
    public class SequenceTests
    {
        private string _swimmerString = "🏊";
        private UnicodeSequence _swimmer = new UnicodeSequence("U+1F3CA");
        private string _femaleSwimmerString = "🏊‍♀️";
        private UnicodeSequence _femaleSwimmer = new UnicodeSequence("U+1F3CA U+200D U+2640 U+FE0F");

        [TestMethod]
        public void TestSequenceEquality()
        {
            Assert.AreEqual(_swimmer.AsString, _swimmerString);
            Assert.IsTrue(_swimmer.Codepoints.SequenceEqual(_swimmerString.Codepoints()));
            Assert.AreEqual(_femaleSwimmer.AsString, _femaleSwimmerString);

            Assert.IsFalse(_swimmer.Equals((string)null));
            Assert.IsTrue(_swimmer.Equals(_swimmer));

            Assert.IsFalse(Equals(_swimmer, null));
            Assert.IsTrue(Equals(_swimmer, _swimmer));
        }

        [TestMethod]
        public void TestSequenceConstruction()
        {
            var testSequence = new UnicodeSequence(0x1F3CA, 0x200D, 0x2640, 0xFE0F);

            // These should all be equivalent methods of initializing the same value
            Assert.AreEqual(testSequence, new UnicodeSequence("U+1F3CA U+200D U+2640 U+FE0F"));
            Assert.AreEqual(testSequence, new UnicodeSequence("1F3CA 200D 2640 FE0F"));
            Assert.AreEqual(testSequence, new UnicodeSequence("1F3CA,200D,2640,FE0F"));
            Assert.AreEqual(testSequence, new UnicodeSequence("1F3CA, 200D, 2640 FE0F"));
        }

        [TestMethod]
        public void TestRangeConstruction()
        {
            Assert.AreEqual(new UnicodeSequence("1F3CA 1F3CB 1F3CC"), new UnicodeSequence("1F3CA-1F3CC"));
        }

        [TestMethod]
        public void TestSequenceMembership()
        {
            Assert.IsTrue(_femaleSwimmer.Contains(_swimmer.Codepoints.First()));
        }

        [TestMethod]
        public void TestSequenceCompareTo()
        {
            Assert.IsTrue(_swimmer.CompareTo(_femaleSwimmer) != 0);
            Assert.IsTrue(_swimmer.CompareTo(_swimmer) == 0);
        }

        [TestMethod]
        public void TestSequenceSame()
        {
            Assert.IsFalse(_swimmer == null);
            Assert.IsFalse(null == _swimmer);
            Assert.IsFalse(_swimmer == _femaleSwimmer);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(_swimmer == _swimmer);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [TestMethod]
        public void TestSequenceNotSame()
        {
            Assert.IsTrue(_swimmer != null);
            Assert.IsTrue(null != _swimmer);
            Assert.IsTrue(_swimmer != _femaleSwimmer);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsFalse(_swimmer != _swimmer);
#pragma warning restore CS1718 // Comparison made to same variable
        }
    }
}
