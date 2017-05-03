using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSmart.Unicode;

namespace UnicodeTests
{
    [TestClass]
    public class EmojiTests
    {
        [TestMethod]
        public void TestEmojiGeneration()
        {
            Assert.AreEqual("😀", Emoji.GrinningFace.ToString(), $"Emoji comparison failed. Expected 😀, but found {Emoji.GrinningFace.ToString()}");
            Assert.AreEqual("👩‍🔧", Emoji.Combine(Emoji.Woman, Emoji.Wrench), $"Emoji comparison failed. Expected 👩‍🔧, but found {Emoji.Combine(Emoji.Woman, Emoji.Wrench)}");
        }

        [TestMethod]
        public void TestEmojiDetection()
        {
            string[] singularEmoji = new []
            {
                "😀",
                "👺",
                "😶"
            };

            string[] combinedEmoji = new []
            {
                "👩‍🔧",
                "👩‍🔬"
            };

            string[] threeEmojiSymbols = new []
            {
                "😀😀😀",
                "👩‍🔧😀😀",
                "👩‍🔧👩‍🔧😀",
                "👩‍🔧👩‍🔧👩‍🔧"
            };

            string[] mixedEmojiDigits = new []
            {
                "😀12",
                "1😀2",
                "12😀"
            };

            string[] mixedEmojiText = new[]
           {
                "😀ab",
                "a😀b",
                "ab😀"
            };

            string[] nonEmojiText = new []
            {
                "123",
                "abc",
            };

            //digits on a keypad followed by the variation selector are emoji
            string[] keycaps = new[]
            {
                "1️⃣",
                "*️⃣"
            };

            string[] punctuation = new[]
            {
                ".",
                "!",
                "?",
                "#",
                "*"
            };

            Assert.IsTrue(singularEmoji.All(x => Emoji.IsEmoji(x)), "Singular emoji failed generic IsEmoji test!");
            Assert.IsTrue(singularEmoji.All(x => Emoji.IsEmoji(x, 1)), "Singular emoji failed IsEmoji test with symbol limit of 1!");

            Assert.IsTrue(combinedEmoji.All(x => Emoji.IsEmoji(x)), "Combined emoji failed generic IsEmoji test!");
            Assert.IsTrue(combinedEmoji.All(x => Emoji.IsEmoji(x, 1)), "Combined emoji failed IsEmoji test with symbol limit of 1!");

            Assert.IsTrue(threeEmojiSymbols.All(x => Emoji.IsEmoji(x)), "Three emoji failed generic IsEmoji test!");

            Assert.IsFalse(threeEmojiSymbols.Any(x => Emoji.IsEmoji(x, 1)), "Three emoji incorrectly detected as 1 emoji or less!");
            Assert.IsFalse(threeEmojiSymbols.Any(x => Emoji.IsEmoji(x, 1)), "Three emoji incorrectly detected as 2 emoji or less!");
            Assert.IsTrue(threeEmojiSymbols.All(x => Emoji.IsEmoji(x, 3)), "Three emoji not detected as 3 emoji or less!");
            Assert.IsTrue(threeEmojiSymbols.All(x => Emoji.IsEmoji(x, 4)), "Three emoji not detected as 4 emoji or less!");

            Assert.IsFalse(mixedEmojiText.Any(x => Emoji.IsEmoji(x)), "Mixed text incorrectly detected as emoji!");
            Assert.IsFalse(mixedEmojiText.Any(x => Emoji.IsEmoji(x, 3)), "Mixed text incorrectly detected as three or less emoji!");

            Assert.IsFalse(mixedEmojiDigits.Any(x => Emoji.IsEmoji(x)), "Mixed digits incorrectly detected as emoji!");
            Assert.IsFalse(mixedEmojiDigits.Any(x => Emoji.IsEmoji(x, 3)), "Mixed digits incorrectly detected as three or less emoji!");

            Assert.IsFalse(nonEmojiText.Any(x => Emoji.IsEmoji(x)), "Text incorrectly detected as emoji!");
            Assert.IsFalse(nonEmojiText.Any(x => Emoji.IsEmoji(x, 3)), "Text incorrectly detected as three or less emoji!");

            Assert.IsTrue(keycaps.All(x => Emoji.IsEmoji(x)), "Keycaps (digit + vs) not detected as emoji!");
            Assert.IsTrue(keycaps.All(x => Emoji.IsEmoji(x, 1)), "Keycaps (digit + vs) not detected as single emoji!");

            Assert.IsFalse(punctuation.Any(x => Emoji.IsEmoji(x)), "Punctuation incorrectly detected as emoji!");
        }
    }
}
