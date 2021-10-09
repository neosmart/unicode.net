using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSmart.Unicode;

namespace UnicodeTests
{
    [TestClass]
    public class SingleEmojiTests
    {
        [TestMethod]
        public void TestSingleEmojiSame()
        {
            Assert.IsFalse(Emoji.AbButtonBloodType == null);
            Assert.IsFalse(null == Emoji.AbButtonBloodType);
            Assert.IsFalse(Emoji.AbButtonBloodType == Emoji.Person);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(Emoji.AbButtonBloodType == Emoji.AbButtonBloodType);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [TestMethod]
        public void TestSingleEmojiNotSame()
        {
            Assert.IsTrue(Emoji.AbButtonBloodType != null);
            Assert.IsTrue(null != Emoji.AbButtonBloodType);
            Assert.IsTrue(Emoji.AbButtonBloodType != Emoji.Person);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsFalse(Emoji.AbButtonBloodType != Emoji.AbButtonBloodType);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [TestMethod]
        public void TestSingleEmojiEquality()
        {
            Assert.IsFalse(Emoji.AbButtonBloodType.Equals(null));
            Assert.IsTrue(Emoji.AbButtonBloodType.Equals(Emoji.AbButtonBloodType));

            Assert.IsFalse(Equals(Emoji.AbButtonBloodType, null));
            Assert.IsTrue(Equals(Emoji.AbButtonBloodType, Emoji.AbButtonBloodType));
        }

        [TestMethod]
        public void TestSingleEmojiCompareTo()
        {
            Assert.IsFalse(Emoji.AbButtonBloodType.CompareTo(Emoji.AbButtonBloodType) != 0);
        }

        [TestMethod]
        public void UnicodeSortOrder()
        {
            // While the unicode sequence for the Thinking Face emoji is U+1F914 and the sequence for
            // Zipper Mouth Face is U+1F910, the Thinking Face emoji should always come directly before
            // it in a sorted list.
            Assert.IsTrue(Emoji.ThinkingFace.Sequence > Emoji.ZipperMouthFace.Sequence);
            Assert.IsTrue(Emoji.ThinkingFace < Emoji.ZipperMouthFace);
        }
    }
}
