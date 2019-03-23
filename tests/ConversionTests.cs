using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSmart.Unicode;
using System.Linq;
using System.Text;

namespace UnicodeTests
{
    [TestClass]
    public class ConversionTests
    {
        private string[] _tests =
        {
            "abcdefg",
            "1234567890",
            "ش	غ	ظ	ذ	خ	ث	ت	س	ر	ق	ض	ف	ع	ص	ن	م	ل	ك	ي	ط	ح	ز	و	ه	د	ج	ب	ا",
            "α  ▪  β  ▪  γ  ▪  δ  ▪  ϵ  ▪  ζ  ▪  η  ▪  θ  ▪  ι  ▪  κ  ▪  λ  ▪  μ  ▪  ν  ▪  ξ  ▪  ο  ▪  π  ▪  ρ  ▪  σ  ▪  τ  ▪  υ  ▪  ϕ  ▪  χ  ▪  ψ  ▪  ω",
            "😅😂😓"
        };

        [TestMethod]
        public void UnicodeSequenceToStringConversion()
        {
            foreach (var test in _tests)
            {
                var codepoints = test.Codepoints();
                var sequence = new UnicodeSequence(codepoints);
                Assert.AreEqual(test, sequence.AsString);
            }
        }

        [TestMethod]
        public void Utf32ByteConversion()
        {
            foreach (var test in _tests)
            {
                var sequence = test.AsUnicodeSequence();
                var utf32 = sequence.AsUtf32Bytes();

                Assert.AreEqual(test, Encoding.UTF32.GetString(utf32.ToArray()));
            }
        }

        [TestMethod]
        public void Utf16ByteConversion()
        {
            foreach (var test in _tests)
            {
                var sequence = test.AsUnicodeSequence();
                var utf16 = sequence.AsUtf16Bytes();

                Assert.AreEqual(test, Encoding.Unicode.GetString(utf16.ToArray()));
            }
        }

        [TestMethod]
        public void Utf8ByteConversion()
        {
            foreach (var test in _tests)
            {
                var sequence = test.AsUnicodeSequence();
                var utf8 = sequence.AsUtf8();

                var encoding = new UTF8Encoding(false);
                Assert.AreEqual(test, encoding.GetString(utf8.ToArray()));
            }
        }
    }
}
