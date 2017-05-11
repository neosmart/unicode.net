using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NeoSmart.Unicode
{
    //We hereby declare emoji to be a zero plural marker noun (in short, "emoji" is both the singular and the plural form)
    //this class refers to emoji in the singular
    public class SingleEmoji : IEqualityComparer<SingleEmoji>, IComparable<SingleEmoji>
    {
        public readonly UnicodeSequence Sequence;
        public readonly string Name;
        public readonly string[] SearchTerms;
        public readonly int SortOrder;

        public SingleEmoji(UnicodeSequence sequence, string name, string[] searchTerms, int sortOrder)
        {
            Sequence = sequence;
            Name = name;
            SearchTerms = searchTerms;
            SortOrder = sortOrder;
        }

        public int CompareTo(SingleEmoji other)
        {
            return SortOrder.CompareTo(other.SortOrder);
        }

        public bool Equals(SingleEmoji x, SingleEmoji y)
        {
            return Sequence == y.Sequence;
        }

        public int GetHashCode(SingleEmoji obj)
        {
            return Sequence.GetHashCode();
        }

        public static bool operator==(SingleEmoji a, SingleEmoji b)
        {
            return a.Sequence == b.Sequence;
        }

        public static bool operator !=(SingleEmoji a, SingleEmoji b)
        {
            return a.Sequence != b.Sequence;
        }

        public override bool Equals(object obj)
        {
            if (obj is SingleEmoji)
            {
                return Equals(this, obj as SingleEmoji);
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
    }
}
