using System.Collections.Generic;
using System.Linq;
using NeoSmart.Collections;

namespace NeoSmart.Unicode
{
    public class MultiRange
    {
        private readonly SortedList<Range> _ranges = new ();
        public IReadOnlyList<Range> Ranges => (IReadOnlyList<Range>) _ranges;

        public MultiRange(params string[] ranges)
        {
            _ranges.AddRange(ranges.Select(r => new Range(r)));
        }

        public MultiRange(params Range[] ranges)
        {
            _ranges.AddRange(ranges);
        }

        public MultiRange(IEnumerable<Range> ranges)
        {
            _ranges.AddRange(ranges);
        }

        public bool Contains(Codepoint codepoint)
        {
            var index = _ranges.IndexOf(new Range(codepoint));
            if (index > 0)
            {
                return true;
            }
            // No match, value is complement of Count or next greatest index
            index = ~index;
            if (index == 0)
            {
                return false;
            }
            // In case of range including this codepoint...
            return _ranges[index - 1].Contains(codepoint)
                // and in case of range starting with this codepoint
                || index < _ranges.Count && _ranges[index].Contains(codepoint);
        }
    }
}
