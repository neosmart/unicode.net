using System;
using System.Linq;
using System.Text;

namespace NeoSmart.Unicode
{
    // We hereby declare emoji to be a zero plural marker noun (in short, "emoji" is both the
    // singular and the plural form). This class refers to emoji in the singular.
    public struct SingleEmoji : IComparable<SingleEmoji>, IEquatable<SingleEmoji>
    {
        static readonly string[] NoTerms = new string[] { };
        public readonly UnicodeSequence Sequence;
        public readonly string Name;
        public readonly string[] SearchTerms;
        public readonly int SortOrder;
        public readonly string Group;
        public readonly string Subgroup;

        public SingleEmoji(UnicodeSequence sequence, string name = "", string[] searchTerms = null, int sortOrder = -1, string group = "", string subgroup = "")
        {
            Sequence = sequence;
            Name = name;
            SearchTerms = searchTerms ?? NoTerms;
            SortOrder = sortOrder;
            Group = group;
            Subgroup = subgroup;
        }

        public int CompareTo(SingleEmoji other)
        {
            if (SortOrder < 0)
            {
                return Sequence.CompareTo(other.Sequence);
            }
            else
            {
                return SortOrder.CompareTo(other.SortOrder);
            }
        }

        public static bool operator ==(SingleEmoji a, SingleEmoji b)
        {
            return a.Sequence == b.Sequence;
        }

        public static bool operator !=(SingleEmoji a, SingleEmoji b)
        {
            return !(a == b);
        }

        public static bool operator <(SingleEmoji a, SingleEmoji b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(SingleEmoji a, SingleEmoji b)
        {
            return a.CompareTo(b) > 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is SingleEmoji emoji)
            {
                return Equals(emoji);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Sequence.GetHashCode();
        }

        public override string ToString()
        {
            return Encoding.Unicode.GetString(Sequence.AsUtf16Bytes().ToArray());
        }

        public bool Equals(SingleEmoji other)
        {
            return Sequence.Equals(other.Sequence);
        }
    }
}
