using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace NeoSmart.Unicode
{
    public class UnicodeSequence : IComparable<UnicodeSequence>, IEquatable<UnicodeSequence>
    {
        Codepoint[] Codepoints;
        
        private UnicodeSequence()
        {

        }

        public UnicodeSequence(string range)
        {
            if (range.Contains("-"))
            {
                var values = range.Split('-');
                Codepoint begin = UInt32.Parse(values[0], System.Globalization.NumberStyles.HexNumber);

                if (values.Length == 1)
                {
                    Codepoints = new Codepoint[1] { begin };
                }
                else if (values.Length == 2)
                {
                    UInt32 end = UInt32.Parse(values[0], System.Globalization.NumberStyles.HexNumber);
                    Codepoints = new Codepoint[end - begin + 1];
                    for (int i = 0; begin + i <= end; ++i)
                    {
                        Codepoints[i] = new Codepoint(begin + i);
                    }
                }
                else
                {
                    throw new InvalidRangeException();
                }
            }
            else
            {
                var values = range.Split(',');
                Codepoints = values.Select(x => new Codepoint(UInt32.Parse(values[0], System.Globalization.NumberStyles.HexNumber))).ToArray();
            }
        }

        public IEnumerable<UInt32> AsUtf32
        {
            get
            {
                foreach (var cp in Codepoints)
                {
                    yield return cp.AsUtf32;
                }
            }
        }

        public IEnumerable<byte> AsUtf32Bytes
        {
            get
            {
                foreach (var u32 in AsUtf32)
                {
                    //little endian byte order
                    yield return (byte)(u32 & 0xFF);
                    yield return (byte)((u32 >> 8) & 0xFF);
                    yield return (byte)((u32 >> 16) & 0xFF);
                    yield return (byte)(u32 >> 24);
                }
            }
        }

        public IEnumerable<UInt16> AsUtf16
        {
            get
            {
                foreach (var cp in Codepoints)
                {
                    foreach (var us in cp.AsUtf16)
                    {
                        yield return us;
                    }
                }
            }
        }

        public IEnumerable<byte> AsUtf16Bytes
        {
            get
            {
                foreach (var us in AsUtf16)
                {
                    //little endian byte order
                    yield return (byte)(us & 0xFF);
                    yield return (byte)(us >> 8);
                }
            }
        }

        public IEnumerable<byte> AsUtf8
        {
            get
            {
                foreach (var cp in Codepoints)
                {
                    foreach (var b in cp.AsUtf8)
                    {
                        yield return b;
                    }
                }
            }
        }

        public int CompareTo(UnicodeSequence other)
        {
            for (int i = 0; i < Math.Min(Codepoints.Length, other.Codepoints.Length); ++i)
            {
                if (Codepoints[i] != other.Codepoints[i])
                {
                    return Codepoints[i].CompareTo(other.Codepoints[i]);
                }
            }
            if (Codepoints.Length == other.Codepoints.Length)
            {
                //same codepoint sequence
                return 0;
            }
            if (Codepoints.Length < other.Codepoints.Length)
            {
                return -(int)other.Codepoints[Codepoints.Length].AsUtf32;
            }
            return (int)Codepoints[other.Codepoints.Length].AsUtf32;
        }

        public bool Equals(UnicodeSequence other)
        {
            if (base.Equals(other))
            {
                return true;
            }

            if (Codepoints.Length != other.Codepoints.Length)
            {
                return false;
            }

            for (int i = 0; i < Codepoints.Length; ++i)
            {
                if (Codepoints[i] != other.Codepoints[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator== (UnicodeSequence a, UnicodeSequence b)
        {
            return a.Equals(b);
        }
        
        public static bool operator!= (UnicodeSequence a, UnicodeSequence b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object b)
        {
            if (b is UnicodeSequence)
            {
                return Equals(b as UnicodeSequence);
            }
            return base.Equals(b);
        }

        public override int GetHashCode()
        {
            return Codepoints.GetHashCode();
        }
    }
}
