using System;
using System.Collections.Generic;
using System.Text;

namespace NeoSmart.Unicode
{
    public static class Codepoints
    {
        /// <summary>
        /// The right-to-left mark
        /// </summary>
        public static Codepoint RLM { get; private set; } = new Codepoint("U+200F");

        /// <summary>
        /// The left-to-right mark
        /// </summary>
        public static Codepoint LRM { get; private set; } = new Codepoint("U+200E");
    }
}
