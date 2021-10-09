using System;
using System.Collections.Generic;

namespace NeoSmart.Unicode
{
    /// <summary>
    /// A <c>Range</c> constitutes a range of <see>Codepoint</see> defined by the
    /// <c>Begin</c> and <c>End</c> values, both of which are inclusive.
    /// </summary>
    public class Range
    {
        /// <summary>
        /// The first codepoint in the range, inclusive.
        /// </summary>
        public readonly Codepoint Begin;
        /// <summary>
        /// The last codepoint in the range, inclusive.
        /// </summary>
        public readonly Codepoint End;

        /// <summary>
        /// Create a range constituting <c>Begin</c> and <c>End</c> codepoints. The two
        /// values may be the same, but <paramref name="begin"/> must be less than or
        /// equal to <paramref name="end"/>
        /// </summary>
        public Range(Codepoint begin, Codepoint end)
        {
            Begin = begin;
            End = end;
        }

        /// <summary>
        /// Create a range constituting a single codepoint (<c>Begin == End</c>).
        /// </summary>
        public Range(Codepoint value)
        {
            Begin = value;
            End = value;
        }

        public bool Contains(Codepoint codepoint)
        {
            return codepoint >= Begin && codepoint <= End;
        }

        static readonly string[] RangeSplit = new[] { "-", "–", "—", ".." };
        // Either a single hex codepoint or two separated by a hyphen

        /// <summary>
        /// Create a range from a string (hexadecimal) description of the range. A range
        /// may be a single codepoint (in which case <c>Begin == End</c>) or a start and
        /// end codepoint separated by a hyphen or two dots.
        ///
        /// Examples of valid ranges: <c>Range("0030..0039")</c>, <c>Range("0040")</c>,
        /// and <c>Range("0600–06FF")</c>
        /// </summary>
        public Range(string range)
        {
            // These are all different hyphens used on Wikipedia and in the UTR
            var values = range.Split(RangeSplit, StringSplitOptions.RemoveEmptyEntries);
            Begin = UInt32.Parse(values[0], System.Globalization.NumberStyles.HexNumber);

            if (values.Length == 1)
            {
                End = Begin;
            }
            else if (values.Length == 2)
            {
                End = UInt32.Parse(values[1], System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                throw new InvalidRangeException();
            }
        }

        public IEnumerable<UInt32> AsUtf32Sequence
        {
            get
            {
                for (UInt32 i = 0; Begin + i <= End; ++i)
                {
                    yield return new Codepoint(Begin + i).AsUtf32();
                }
            }
        }

        public IEnumerable<UInt16> AsUtf16Sequence
        {
            get
            {
                for (var i = 0; Begin + i <= End; ++i)
                {
                    foreach (var utf16 in new Codepoint(Begin + i).AsUtf16())
                    {
                        yield return utf16;
                    }
                }
            }
        }

        public IEnumerable<byte> AsUtf8Sequence
        {
            get
            {
                for (var i = 0; Begin + i <= End; ++i)
                {
                    foreach (var utf8 in new Codepoint(Begin + i).AsUtf8())
                    {
                        yield return utf8;
                    }
                }
            }
        }
    }
}
