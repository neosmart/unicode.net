using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Runtime.InteropServices;

namespace NeoSmart.Unicode
{
    public readonly struct Codepoint : IComparable<Codepoint>, IComparable<UInt32>, IEquatable<Codepoint>,
        IEquatable<string>, IComparable<string>, IEquatable<char>
    {
        public readonly UInt32 Value;

        public Codepoint(UInt32 value)
        {
            Value = value;
        }

        public Codepoint(long value)
            : this((UInt32)value)
        { }

        /// <summary>
        /// Create a unicode codepoint from hexadecimal representation, supporting U+xxxx and 0xYYYY notation.
        /// </summary>
        /// <param name="hexValue"></param>
#if NETSTANDARD2_1_OR_GREATER
        public Codepoint(ReadOnlySpan<char> hexValue)
        {
            if ((hexValue.StartsWith("0x") || hexValue.StartsWith("U+") || hexValue.StartsWith("u+")))
            {
                hexValue = hexValue.Slice(2);
            }
            if (!UInt32.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.CurrentCulture.NumberFormat, out Value))
            {
                throw new UnsupportedCodepointException();
            }
        }
#else
        public Codepoint(string hexValue)
        {
            if ((hexValue.StartsWith("0x") || hexValue.StartsWith("U+") || hexValue.StartsWith("u+")))
            {
                hexValue = hexValue.Substring(2);
            }
            if (!UInt32.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.CurrentCulture.NumberFormat, out Value))
            {
                throw new UnsupportedCodepointException();
            }
        }
#endif

        public UInt32 AsUtf32() => Value;

        /// <summary>
        /// Returns an iterator that will enumerate over the big endian bytes in the UTF32 encoding of this codepoint.
        /// </summary>
        public IEnumerable<byte> AsUtf32Bytes()
        {
            var utf32 = AsUtf32();
            var b1 = (byte)(utf32 >> 24);
            yield return b1;
            var b2 = (byte)((utf32 & 0x00FFFFFF) >> 16);
            yield return b2;
            var b3 = (byte)(((UInt16)utf32) >> 8);
            yield return b3;
            var b4 = (byte)utf32;
            yield return b4;
        }

        public void AsUtf32Bytes(Span<byte> dest)
        {
            if (dest.Length < 4)
            {
                throw new ArgumentException("dest must be a 4-byte array");
            }

            var utf32 = Value;
            dest[0] = (byte)(utf32 >> 24);
            dest[1] = (byte)((utf32 & 0x00FFFFFF) >> 16);
            dest[2] = (byte)(((UInt16)utf32) >> 8);
            dest[3] = (byte)utf32;
        }

        // Reference: https://en.wikipedia.org/wiki/UTF-16
        public IEnumerable<UInt16> AsUtf16()
        {
            // U+0000 to U+D7FF and U+E000 to U+FFFF
            if (Value <= 0xFFFF)
            {
                yield return (UInt16)Value;
            }
            // U+10000 to U+10FFFF
            else if (Value >= 0x10000 && Value <= 0x10FFFF)
            {
                UInt32 newVal = Value - 0x010000; // leaving 20 bits
                UInt16 high = (UInt16)((newVal >> 10) + 0xD800);
                System.Diagnostics.Debug.Assert(high <= 0xDBFF && high >= 0xD800);
                yield return high;

                UInt16 low = (UInt16)((newVal & 0x03FF) + 0xDC00);
                System.Diagnostics.Debug.Assert(low <= 0xDFFF && low >= 0xDC00);
                yield return low;
            }
            else
            {
                throw new UnsupportedCodepointException();
            }
        }

        public int AsUtf16(Span<UInt16> dest)
        {
            // U+0000 to U+D7FF and U+E000 to U+FFFF
            if (Value <= 0xFFFF)
            {
                dest[0] = (UInt16) Value;
                return 1;
            }
            // U+10000 to U+10FFFF
            else if (Value >= 0x10000 && Value <= 0x10FFFF)
            {
                UInt32 newVal = Value - 0x010000; // leaving 20 bits
                UInt16 high = (UInt16)((newVal >> 10) + 0xD800);
                System.Diagnostics.Debug.Assert(high <= 0xDBFF && high >= 0xD800);
                dest[0] = high;

                UInt16 low = (UInt16)((newVal & 0x03FF) + 0xDC00);
                System.Diagnostics.Debug.Assert(low <= 0xDFFF && low >= 0xDC00);
                dest[1] = low;
                return 2;
            }
            else
            {
                throw new UnsupportedCodepointException();
            }
        }

        /// <summary>
        /// Returns an iterator that will enumerate over the little endian bytes in the UTF16 encoding of this codepoint.
        /// </summary>
        public IEnumerable<byte> AsUtf16Bytes()
        {
            var utf16 = AsUtf16();
            foreach (var u16 in utf16)
            {
                var high = (byte)(u16 >> 8);
                yield return high;
                var low = (byte)u16;
                yield return low;
            }
        }

        /// <summary>
        /// Returns an iterator that will enumerate over the little endian bytes in the UTF16 encoding of this codepoint.
        /// </summary>
        public int AsUtf16Bytes(Span<byte> dest)
        {
            // U+0000 to U+D7FF and U+E000 to U+FFFF
            if (Value <= 0xFFFF)
            {
                dest[0] = (byte) (Value);
                dest[1] = (byte) (Value >> 8);
                return 2;
            }

            // U+10000 to U+10FFFF
            if (Value >= 0x10000 && Value <= 0x10FFFF)
            {
                UInt32 newVal = Value - 0x010000; // leaving 20 bits
                UInt16 high = (UInt16)((newVal >> 10) + 0xD800);
                System.Diagnostics.Debug.Assert(high <= 0xDBFF && high >= 0xD800);
                dest[0] = (byte) (high);
                dest[1] = (byte) (high >> 8);

                UInt16 low = (UInt16)((newVal & 0x03FF) + 0xDC00);
                System.Diagnostics.Debug.Assert(low <= 0xDFFF && low >= 0xDC00);
                dest[2] = (byte) (low);
                dest[3] = (byte) (low >> 8);
                return 4;
            }

            throw new UnsupportedCodepointException();
        }

        // https://en.wikipedia.org/wiki/UTF-8
        public IEnumerable<byte> AsUtf8()
        {
            // Up to 7 bits
            if (Value <= 0x007F)
            {
                yield return (byte)Value;
                yield break;
            }

            // Up to 11 bits
            if (Value <= 0x07FF)
            {
                yield return (byte)(0b11000000 | (0b00011111 & (Value >> 6))); // tag + upper 5 bits
                yield return (byte)(0b10000000 | (0b00111111 & Value)); // tag + lower 6 bits
                yield break;
            }

            // Up to 16 bits
            if (Value <= 0x0FFFF)
            {
                yield return (byte)(0b11100000 | (0b00001111 & (Value >> 12))); // tag + upper 4 bits
                yield return (byte)(0b10000000 | (0b00111111 & (Value >> 6))); // tag + next 6 bits
                yield return (byte)(0b10000000 | (0b00111111 & Value)); // tag + last 6 bits
                yield break;
            }

            // Up to 21 bits
            if (Value <= 0x1FFFFF)
            {
                yield return (byte)(0b11110000 | (0b00000111 & (Value >> 18))); // tag + upper 3 bits
                yield return (byte)(0b10000000 | (0b00111111 & (Value >> 12))); // tag + next 6 bits
                yield return (byte)(0b10000000 | (0b00111111 & (Value >> 6))); // tag + next 6 bits
                yield return (byte)(0b10000000 | (0b00111111 & Value)); // tag + last 6 bits
                yield break;
            }

            throw new UnsupportedCodepointException();
        }

        public Codepoint FromUtf32(UInt32 value)
        {
            return new Codepoint(value);
        }

        private static readonly Range Utf16SurrogateHigh = new Range(0xD800, 0xDBFF);
        private static readonly Range Utf16SurrogateLow = new Range(0xDC00, 0xDFFF);

        public int Utf16ByteCount => (Value <= 0xFFFF) ? 2 : 4;

        public Codepoint FromUtf16(UInt16 word1, UInt16 word2 = 0)
        {
            if (word1 >= 0xD800 && word1 <= 0xDBFF)
            {
                // word1 is a leading surrogate pair
                if (!(word2 >= 0xDC00 && word2 <= 0xDFFF))
                {
                    // word2 is not a trailing surrogate pair
                    throw new UnsupportedCodepointException("Invalid UTF-16 surrogate pair!");
                }
            }
            else if (word2 != 0)
            {
                // word1 is not a surrogate pair, but two words provided
                throw new UnsupportedCodepointException("word1 is not a leading surrogate pair but word2 also provided");
            }
            if ((word1 >= 0xDC00 && word1 <= 0xDFFF))
            {
                // word1 is a trailing surrogate pair
                throw new UnsupportedCodepointException("word1 is a trailing surrogate pair!");
            }

            // Reference: https://unicode.org/faq/utf_bom.html#utf16-4
            const Int32 LEAD_OFFSET = 0xD800 - (0x10000 >> 10);
            const Int32 SURROGATE_OFFSET = (0x10000 - (0xD800 << 10) - 0xDC00);

            Int16 lead = (Int16) (LEAD_OFFSET + (word1 >> 10));
            Int16 trail = (Int16) (0xDC00 + (word1 & 0x3FF));

            UInt32 codepoint = (UInt32) ((lead << 10) + trail + SURROGATE_OFFSET);
            return new Codepoint(codepoint);
        }

        public int CompareTo(Codepoint other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(UInt32 other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(Codepoint other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            return obj is Codepoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Codepoint a, Codepoint b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(Codepoint a, Codepoint b)
        {
            return a.Value != b.Value;
        }

        public static bool operator <(Codepoint a, Codepoint b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >(Codepoint a, Codepoint b)
        {
            return a.Value > b.Value;
        }

        public static bool operator >=(Codepoint a, Codepoint b)
        {
            return a.Value >= b.Value;
        }

        public static bool operator <=(Codepoint a, Codepoint b)
        {
            return a.Value <= b.Value;
        }

        public static implicit operator UInt32(Codepoint codepoint)
        {
            return codepoint.Value;
        }

        public static implicit operator Codepoint(UInt32 value)
        {
            return new Codepoint(value);
        }

        public override string ToString()
        {
            return $"U+{Value.ToString("X")}";
        }

        public string AsString()
        {
            Span<byte> bytes = stackalloc byte[4];
            var count = AsUtf16Bytes(bytes);
            unsafe
            {
                fixed (byte *ptr = bytes)
                {
                    return Encoding.Unicode.GetString(ptr, count);
                }
            }
        }

        public bool IsIn(Range range)
        {
            return range.Contains(this);
        }

        public bool IsIn(MultiRange multirange)
        {
            return multirange.Contains(this);
        }

        public bool Equals(string? other)
        {
            return other is not null && other == AsString();
        }

        public int CompareTo(string? other)
        {
            if (other is null)
            {
                return 1;
            }

            Span<UInt16> words = stackalloc UInt16[2];
            var count = AsUtf16(words);
            words = words.Slice(0, count);
            var chars = MemoryMarshal.Cast<UInt16, char>(words);
            return chars.SequenceCompareTo(other.AsSpan());
        }

        public bool Equals(char other)
        {
            Span<UInt16> words = stackalloc UInt16[2];
            return AsUtf16(words) == 1 && words[0] == other;
        }

        public static bool operator ==(Codepoint a, string b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Codepoint a, string b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(Codepoint a, char b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Codepoint a, char b)
        {
            return !a.Equals(b);
        }
    }
}
