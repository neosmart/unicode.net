using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoSmart.Unicode
{
    /// <summary>
    /// A UnicodeSequence is a combination of one or more codepoints.
    /// </summary>
    public struct UnicodeSequence : IComparable<UnicodeSequence>, IEquatable<UnicodeSequence>, IEquatable<string>
    {
        private static readonly char[] SequenceSplitChars = new[] { ',', ' ' };

        private readonly Codepoint[] _codepoints;
        public IReadOnlyList<Codepoint> Codepoints => _codepoints;

        public UnicodeSequence(string sequence)
        {
            if (sequence.Contains("-"))
            {
                var values = sequence.Split('-');

                if (values.Length == 2)
                {
                    Codepoint begin = new Codepoint(values[0]);
                    Codepoint end = new Codepoint(values[1]);
                    if (end.Value < begin.Value)
                    {
                        throw new InvalidRangeException();
                    }
                    _codepoints = new Codepoint[end.Value - begin.Value + 1];
                    for (int i = 0; begin.Value + i <= end.Value; ++i)
                    {
                        _codepoints[i] = new Codepoint(begin.Value + i);
                    }
                }
                else
                {
                    throw new InvalidRangeException();
                }
            }
            else
            {
                var values = sequence.Split(SequenceSplitChars, StringSplitOptions.RemoveEmptyEntries);
                _codepoints = values.Select(x => new Codepoint(x)).ToArray();
            }
        }

        // The usage of `params` here should hopefully allocate on the stack for short lengths,
        // making this the most optimized version of the routine.
        public UnicodeSequence(params Codepoint[] codepoints)
        {
            _codepoints = codepoints;
        }

        public UnicodeSequence(IEnumerable<Codepoint> codepoints)
        {
            _codepoints = codepoints.ToArray();
        }

        public bool Contains(Codepoint codepoint)
        {
            return codepoint.In(_codepoints);
        }

        public IEnumerable<uint> AsUtf32()
        {
            foreach (var cp in _codepoints)
            {
                yield return cp.AsUtf32();
            }
        }

        public IEnumerable<byte> AsUtf32Bytes()
        {
            foreach (var u32 in AsUtf32())
            {
                // Little Endian byte order
                yield return (byte)(u32 & 0xFF);
                yield return (byte)((u32 >> 8) & 0xFF);
                yield return (byte)((u32 >> 16) & 0xFF);
                yield return (byte)(u32 >> 24);
            }
        }

        public IEnumerable<UInt16> AsUtf16()
        {
#if NETSTANDARD2_1_OR_GREATER
            var words = new UInt16[2];
#endif
            foreach (var cp in _codepoints)
            {
#if NETSTANDARD2_1_OR_GREATER
                int count = cp.AsUtf16(words);
                yield return words[0];
                if (count == 2)
                {
                    yield return words[1];
                }
#else
                foreach (var u16 in cp.AsUtf16())
                {
                    yield return u16;
                }
#endif
            }
        }

        // Little Endian byte order
        public IEnumerable<byte> AsUtf16Bytes()
        {
#if NETSTANDARD2_1_OR_GREATER
            var bytes = new byte[4];
            foreach (var cp in _codepoints)
            {
                var count = cp.AsUtf16Bytes(bytes);
                for (int i = 0; i < count; ++i)
                {
                    yield return bytes[i];
                }
            }
#else
            foreach (var u16 in AsUtf16())
            {
                yield return (byte)(u16 & 0xFF);
                yield return (byte)(u16 >> 8);
            }
#endif
        }

        public IEnumerable<byte> AsUtf8()
        {
            foreach (var cp in _codepoints)
            {
                foreach (var b in cp.AsUtf8())
                {
                    yield return b;
                }
            }
        }

        public string AsString()
        {
            return Encoding.Unicode.GetString(AsUtf16Bytes().ToArray());
        }

        public int CompareTo(UnicodeSequence other)
        {
            return MemoryExtensions.SequenceCompareTo<Codepoint>(_codepoints, other._codepoints);
        }

        public bool Equals(UnicodeSequence other)
        {
            return MemoryExtensions.SequenceEqual<Codepoint>(_codepoints, other._codepoints);
        }

        public static bool operator ==(UnicodeSequence a, UnicodeSequence b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UnicodeSequence a, UnicodeSequence b)
        {
            return !a.Equals(b);
        }

        public static bool operator <(UnicodeSequence a , UnicodeSequence b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(UnicodeSequence a , UnicodeSequence b)
        {
            return a.CompareTo(b) > 0;
        }

        public override bool Equals(object? b)
        {
            return b is UnicodeSequence other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _codepoints.GetHashCode();
        }

        public bool Equals(string? other)
        {
            return other is not null && other.Codepoints().SequenceEqual(_codepoints);
        }

        public override string ToString()
        {
            return string.Join(" ", Codepoints);
        }
    }
}
