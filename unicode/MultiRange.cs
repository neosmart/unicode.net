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
            // Find the range that starts less than the specified codepoint
            var index = _ranges.IndexOf(new Range(codepoint));
            if (index > 0)
            {
                return true;
            }
            // No match, value is complement of Count or next greatest index
            index = ~index;
            if (index == _ranges.Count)
            {
                return false;
            }
            return _ranges[index].Contains(codepoint);
        }
    }
}
