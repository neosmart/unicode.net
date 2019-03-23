﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Assert.IsFalse(Emoji.AbButtonBloodType == Emoji.Adult);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(Emoji.AbButtonBloodType == Emoji.AbButtonBloodType);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [TestMethod]
        public void TestSingleEmojiNotSame()
        {
            Assert.IsTrue(Emoji.AbButtonBloodType != null);
            Assert.IsTrue(null != Emoji.AbButtonBloodType);
            Assert.IsTrue(Emoji.AbButtonBloodType != Emoji.Adult);
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

            Assert.IsFalse(Emoji.AbButtonBloodType.Equals(null, Emoji.Adult));
            Assert.IsFalse(Emoji.AbButtonBloodType.Equals(Emoji.Adult, null));
            Assert.IsTrue(Emoji.AbButtonBloodType.Equals(Emoji.Adult, Emoji.Adult));
        }

        [TestMethod]
        public void TestSingleEmojiCompareTo()
        {
            Assert.IsFalse(Emoji.AbButtonBloodType.CompareTo(Emoji.AbButtonBloodType) != 0);
        }
    }
}
