namespace NeoSmart.Unicode
{
    public partial class Languages
    {
        /// <summary>
        /// A "language" that's composed of just the 10 Arabic numerals
        /// </summary>
        public static MultiRange ArabicNumerals = new MultiRange("0030..0039");

        /// <summary>
        /// All the Arabic codepoints
        /// </summary>
        public static MultiRange Arabic = new MultiRange("0600–06FF", "0750–077F", "08A0–08FF", "FB50–FDFF", "FE70–FEFF", "10E60–10E7F", "1EE00—1EEFF");

        // See Languages-Emoji.cs for the (messy) implementation of the Emoji coderange
    }
}
