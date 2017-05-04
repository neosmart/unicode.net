using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSmart.Unicode;
using System;
using System.Collections.Generic;
using System.Text;
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
        }

        [TestMethod]
        public void TestSequenceMembership()
        {
            Assert.IsTrue(_femaleSwimmer.Contains(_swimmer.Codepoints.First()));
        }
    }
}
