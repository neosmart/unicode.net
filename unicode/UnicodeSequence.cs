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
        static readonly Codepoint[] NoCodepoints = new Codepoint[] { };
        private readonly Codepoint[] _codepoints;
        public IEnumerable<Codepoint> Codepoints => _codepoints ?? NoCodepoints;

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
                var values = sequence.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

        public IEnumerable<ushort> AsUtf16()
        {
            foreach (var cp in _codepoints)
            {
                foreach (var us in cp.AsUtf16())
                {
                    yield return us;
                }
            }
        }

        public IEnumerable<byte> AsUtf16Bytes()
        {
            foreach (var us in AsUtf16())
            {
                // Little Endian byte order
                yield return (byte)(us & 0xFF);
                yield return (byte)(us >> 8);
            }
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

        public string AsString => Encoding.Unicode.GetString(AsUtf16Bytes().ToArray());

        public int CompareTo(UnicodeSequence other)
        {
            if (_codepoints is null && other._codepoints is null)
            {
                return 0;
            }

            if (_codepoints is null)
            {
                return -1;
            }

            if (other._codepoints is null)
            {
                return 1;
            }

            for (int i = 0; i < Math.Min(_codepoints.Length, other._codepoints.Length); ++i)
            {
                if (_codepoints[i] != other._codepoints[i])
                {
                    return _codepoints[i].CompareTo(other._codepoints[i]);
                }
            }
            if (_codepoints.Length == other._codepoints.Length)
            {
                // The codepoint sequences are the same
                return 0;
            }
            if (_codepoints.Length < other._codepoints.Length)
            {
                return -(int)other._codepoints[_codepoints.Length].AsUtf32();
            }
            return (int)_codepoints[other._codepoints.Length].AsUtf32();
        }

        public bool Equals(UnicodeSequence other)
        {
            if (base.Equals(other))
            {
                return true;
            }

            if (_codepoints.Length != other._codepoints.Length)
            {
                return false;
            }

            for (int i = 0; i < _codepoints.Length; ++i)
            {
                if (_codepoints[i] != other._codepoints[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(UnicodeSequence a, UnicodeSequence b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UnicodeSequence a, UnicodeSequence b)
        {
            return !(a == b);
        }

        public static bool operator <(UnicodeSequence a , UnicodeSequence b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(UnicodeSequence a , UnicodeSequence b)
        {
            return a.CompareTo(b) > 0;
        }

        public override bool Equals(object b)
        {
            if (b is UnicodeSequence)
            {
                return Equals((UnicodeSequence)b);
            }
            return base.Equals(b);
        }

        public override int GetHashCode()
        {
            return _codepoints.GetHashCode();
        }

        public bool Equals(string other)
        {
            return !(other is null) && other.Codepoints().SequenceEqual(_codepoints);
        }
    }
}
