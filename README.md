# This is a fork of [neosmart/unicode.net](https://github.com/neosmart/unicode.net) from [mqudsi](https://github.com/mqudsi)

It adds more properties to the `SingleEmoji` class (click [here](https://github.com/UWPX/unicode.net#class-singleemoji) for their documentation) and lists for all emoji groups.

## Unicode.net- an emoji and text-processing library for .NET

`Unicode.net` is an easy-to-use Unicode text-processing library for dot net, designed to complement the BCL and the `System.String` class, useable on both .NET Framework and .NET Core/UWP (.NET Standard) targets. As an added bonus, `Unicode.net` includes an extra helping of emoji awesomeness 🎉 😊 😄.

Unicode.net is [available on NuGet](https://www.nuget.org/packages/Unicode.net) for all .NET platforms and versions, and is made open source by NeoSmart Technologies under the terms of the MIT License. Contributions are welcomed and appreciated.

.NET is not natively Unicode-aware, while the API has full support for internationalization by using UTF-16 strings aware, capable of passing Unicode-encoded text and carrying out operations involving non-English/non-ASCII text data, the interface is almost exclusively a black box, and the abstraction fails once attempts are made to actually access the underlying string data (i.e. indexing a Unicode string containing non-ASCII data returns individual 16-bit values rather than complete Unicode sequences referring to letters or symbols).

### The `Unicode.net` puzzle pieces

It's best to have a basic understanding of how the various components of the `Unicode.net` library fit together before diving in to the code. `Unicode.net` was purposely designed with the .NET Framework and the C# language in mind, it is designed to *complement*, not supplant, the existing string manipulation functions. Most importantly, `Unicode.net` does not do away with the `string` class, instead, it embraces and extends it with more Unicode goodness (C#'s extension methods make this beautifully easy).

#### A quick Unicode primer

It is unfortunately impossible to use this library without having a very basic understanding of text encoding and Unicode in general. While this section may be extended in the future, for now, a basic understanding of how text must be encoded according to a certain spec to form strings out of a sequence of bytes is a necessary prerequisite. Unicode is one such format, and is the primary standard when it comes to representing non-English content in a standard, binary format.

Currently, any "letter" in any language can be expressed as a sequence of one or more Unicode *codepoints*. A codepoint is the basic building block of a Unicode string, somewhat like how a 8-bit character can be considered the basic building block of an ASCII string. A Unicode codepoint is 32-bits in length, or 4 bytes. Unicode itself is *not* an encoding per-se, but is rather comprised of 3 different encodings: UTF-8, UTF-16 (or what .NET refers to everywhere as Unicode), and UTF-32. These *encode* a single Unicode codepoint out of 8-bit, 16-bit, and 32-bit sequences respectively. Unicode codepoints "small enough" to fit in a single byte can be represented in just one UTF-8 character, just as those "small enough" to fit in 2 bytes can be represented in just one UTF-16 "character," and (currently) any Unicode codepoint can be represented as a single UTF-32 codepoint. But codepoints "too big" to fit in a single 8-bit struct must be "split up" into separate 8-bit components to be represented as UTF-8, and the same for those too big to fit in a 16-bit struct for UTF-16, etc.

A *Unicode sequence* is a, well, sequence of one or more Unicode codepoints (in any of the three Unicode encodings mentioned above). Such a sequence can be used to represent just one symbol (such as the Arabic ﻉ or the see-no-evil 🙈 emoji), or they can be the representation of multiple such letters used to form a sentence (i.e. a `string`). There exists a direct mapping from `UnicodeSequence` to `System.String`, though it is important to note that this mapping is not unique, in that multiple, different `UnicodeSequence` values can map to a single `string` (here the concept of string normalization comes into play, where there is only one "canonical" Unicode representation for any given string, but that is outside the scope of this primer).

#### The basic components of `Unicode.net`

The main classes of Unicode.net have actually already been covered, and they are `Codepoint` and  `UnicodeSequence`. Here's how `Codepoint`, `UnicodeSequence`, and `System.String` fit together:

<img src="https://i.imgur.com/drzRLum.png" width=450/>



And here's how the Unicode `Codepoint` object can be represented in the various UTF-8, UTF-16, and UTF-32 encodings:

<img src="https://i.imgur.com/zAMtCjb.png" width=450/>



And that's all you really need to know to get started!

### Quick `Unicode.net` function reference

The below is only a primer on the primary features of `Unicode.net`, being the interfaces most new developers are most likely to be interested in when first encountering this library. See the complete documentation for the complete API reference.

#### Extension methods for `System.String`:

* `.Codepoints()`, returning `IEnumerable<Codepoint>`, providing a direct mapping between any .NET `string` and the underlying Unicode codepoints.
* `.Letters()`, returning `IEnumerable<string>`, where each `string` is one Unicode "letter". This is the safe way to decompose a `string` into its individual letters, useful for counting letters, checking for the presence of one or more letters in a string, and other such tasks where a naïve implementation would involve an expression along the lines of `foreach (char c in str)`. Since .NET strings are UTF-16-encoded, a `string` instance can be used to safely represent any sequence of one or more Unicode codepoints in UTF-16 format. `str.Letters()` and `str.Codepoints()` can be considered as (and, in fact, are) different representations of the same data.

#### Class `Codepoint`

The `Codepoint` class, representing a single Unicode codepoint in an encoding-agnostic format:

* `Codepoint(string hexValue)`, creating a `Codepoint` instance representing a single Unicode codepoint from its literal form, such as `U+1F431` to represent the codepoint forming the emoji symbol for a cat (🐱),
* `Codepoint(UInt32 value)`, creating a `Codepoint` instance representing a single Unicode codepoint from its 32-bit Unicode value, such as `Codepoint(0x1F431)` to represent the same cat emoji as in the example above,
* `Codepoint.AsUtf32`, returning a single `UInt32` representing the UTF-32 encoding of the equivalent Unicode codepoint,
* `Codepoint.AsUtf16`, returning an `IEnumerable<UInt16>` composed of either one or two `UInt16` values being the UTF-16 representation of the stored Unicode codepoint,
* `Codepoint.AsUtf8`, returning an `IEnumerable<UInt16>` composed of anywhere from one to four `UInt8` values being the UTF-8 representation of the stored Unicode codepoint.
* `Codepoint.AsString()`, returning `string` as a native .NET UTF-16 representation of the `Codepoint`. This `string` value is the *internationalization-safe* equivalent of the .NET native `char` primitive. For ANSI ASCII codepoints, the `Codepoint.AsString()` value is equal to `new string(c)` where `c` is the `char` comprising the letter/codepoint in question (this is the only case wherein a Unicode-aware `Codepoint` and naïve `char` value ever represent the same data).

#### Class `UnicodeSequence`

The `UnicodeSequence` class, representing a combination of a Unicode-encoded string in an encoding-agnostic format, that can be decomposed into its individual `Codepoint` values:

* `UnicodeSequence(string sequence)`, forming a `UnicodeSequence` instance from the string representing of its individual codepoints. The string representation should take the form of a comma-separated list of Unicode codepoints representing the sequence. A `-` can optionally be used to indicate that the `UnicodeSequence` should be formed by all the codepoints in a range.
  Valid string representations of a `UnicodeSequence` include `U+1F469, U+1F3FE, U+200D, U+1F52C`, all of which come together to form the dark-skinned, female scientist emoji, which is actually represented by just a single glyph, composed of a sequence of four Unicode codepoints, each represented by a number of bytes: `👩🏾‍🔬`;
* `UnicodeSequence.Codepoints`, returning `IEnumerable<Codepoint>` and allowing for the decomposition of a `UnicodeSequence` object into its individual Unicode codepoints, which can be further decomposed into bytes via the `Codepoint` class functions mentioned above;
* `UnicodeSequence.AsUtf32`, returning `IEnumerable<UInt32>` to directly decompose a Unicode sequence into its UTF-32 binary representation;
* `UnicodeSequence.AsUtf16`, returning `IEnumerable<UInt16>` to directly decompose a Unicode sequence into its UTF-16 binary representation;
* `UnicodeSequence.AsUtf32`, returning `IEnumerable<UInt8>` to directly decompose a Unicode sequence into its UTF-8 binary representation;
* `UnicodeSequence.AsString`, returning `string` for a direct mapping between `UnicodeSequence` and `System.String`.

### The `Unicode.net` Emoji API

What's the point of a Unicode text-processing library that does not provide an API for dealing with emoji? After all, emoji are probably the single-biggest driver behind Unicode adoption in recent years!

#### Class `Emoji`

The static `Emoji` class is the main entry point for dealing with emoji in `Unicode.net`.

* `Emoji.All`: An ordered list of `SingleEmoji`, sorted by the recommended sort per the Unicode consortium, composed of all the emoji as of [UTR #51](http://unicode.org/reports/tr51/). Depending on your platform and font, not all of these may have visual representations in the form of emoji glyphs. Unsupported emoji on your platform may be rendered as a blank rectangle, indicating the requested codepoint was not found in the font table of the selected font, or as a sequence of several emoji rather than the single-glyph representation indicated in the UTR, as many emoji are composed of several emoji joined with a ZeroWidthJoiner, for example, the female scientist emoji 👩🏾‍🔬 is actually composed of the codepoints for "adult female emoji" (👩), "dark skin tone emoji" (🏾), the zero width joiner, and the "microscope emoji" (🔬), all coming together to form a single glyph. On unsupported platforms, instead of showing up as 👩🏾‍🔬, this might show up as 👩🔬 or a variation thereof.
* `Emoji.Basic`: An ordered list of `SingleEmoji`, comprised of the `Emoji.All` list with all skin tone variations removed (i.e. only the "original," (normally) yellow version of the emoji), in gender-neutral format (where possible). This is harder than it sounds, because some emoji are available in ungendered format only, while others are available in both gendered (i.e. w/ separate male & female emoji) and ungendered form, and others are yet only available gendered format. 
  As a concrete example, there is no `Cook` emoji, only `ManCook` 👨‍🍳 and `WomanCook` 👩‍🍳, so both of those appear in the `Emoji.Basic` list; whereas `ConstructionWorker`, `ManConstructionWorker`, and `WomanConstructionWorker` are all present in the `Emoji.All` list, so only the gender-neutral `ConstructionWorker` emoji is included in the `Emoji.Basic` list.
  All emoji without a valid, single-glyph representation in the latest version of the `Segoe UI Emoji` font (the one shipping with Windows 10 Creators Update, as of this writing) are also omitted from the `Emoji.Basic` list, making it ideal for UI purposes. It can be enumerated as-is and as-ordered for use in a an emoji picker, etc.
* `Emoji.SkinTones`: A subclass containing an enumeration of the valid skin tone modifiers (`Light`, `MediumLight`, `Medium`, `MediumDark`,  and `Dark`, as well as their Fitzpatrick equivalents, `Fitzpatrick12`, `Fitzpatrick3`, `Fitzpatrick4`, `Fitzpatrick5`, and `Fitzpatrick6`).
* `bool Emoji.IsEmoji(string message, int maxSymbolCount)`: a helpful function for UI-rendering purposes that determines whether the passed string `message` is a string composed solely of emoji, optionally specifying the maximum number of *rendered glyphs* allowed in the supplied message. **This isn't the same as the number of Unicode codepoints, the number of bytes, or the number of letters**, and this information can only be determined by use of this helper function. The `IsEmoji` function is most useful when trying to display a string composed exclusively of up to *n* emoji at a larger font, similar to how iMessage, WhatsApp, and Slack will display emoji-only responses up to a certain length in a larger font size.
* The rest of the `Emoji` class is composed of all the emoji in the UTR #51 spec as well as some helpful non-emoji Unicode codepoints (`ZeroWidthJoiner`, `ObjectReplacementCharacter`, and `Keycap`) which come in handy when dealing with the Unicode representation of emoji. The emoji in the class are the same as those in the `Emoji.All` list and are formed by a CamelCased representation of their official names per the UTR #51 spec, such as `Emoji.GrinningFace` and `Emoji.ManFirefighterMediumSkinTone`. Each emoji is an instance of `SingleEmoji` (see below).

#### Class `SingleEmoji`

The `SingleEmoji` class is a representation of a single "emoji," where "emoji" is any unicode sequence comprised of one or more basic emoji sequences that *should* be represented by a single glyph, per the UTR #51 spec. Again, depending on your platform and font and the emoji they support, a `SingleEmoji` *may* either have no representation or be represented as a sequence of one or more individual emoji. 

Important note: this class is called `SingleEmoji` and the "master" emoji class is called `Emoji` because we firmly believe that "emoji" &mdash; as a foreign word derived from the Japanese えもじらんど &mdash; is a *zero plural marker* noun, which is to say, a noun with no plural form distinct from its singular form. **The plural of "emoji" is "emoji" and absolutely never "emojis," which is quite simply not a word at all.**

That said, the `SingleEmoji` class contains all the information needed to represent a single glyph from the UTR spec, and to interact with its individual Unicode codepoints via the Unicode API described elsewhere in these docs:

* `UnicodeSequence Sequence`: the underlying Unicode codepoints that form the emoji in question, for example, `UnicodeSequence("1F474 1F3FC")`, which form the "Old Man w/ Medium Skin Tone" emoji;
* `string Name`: the name of the `SingleEmoji` instance, as derived from the official name in the UTR spec. The name of the `SingleEmoji` instance in the `Emoji` class (and the members of the `Emoji.Basic` and `Emoji.All` lists) is derived from this value. For example, the name of the `SingleEmoji` instance `Emoji.OlderAdultMediumDarkSkinTone` is "older adult: medium-dark skin tone".
* `string[] SearchTerms`: an array of one or more keywords that can be used when implementing a "search for emoji" feature. These keywords will be expanded to include synonyms and visual equivalents over time, but is useable in its present state. Example: `{ "woman", "health", "worker", "medium", "light", "skin", "tone" }`.
* `string[] NoTerms`: If the `string[] SearchTerms` is empty/`null` it points to this.
* `SkinTone[] SkinTones`: A list of `SkinTone` objects that represent the current emoji. Example: `{ SkinTone.NONE }` for "😀" , `{ SkinTone.DARK }` for "👨🏿‍⚕️" and `{ SkinTone.DARK, SkinTone.MEDIUM }` for "🧑🏿‍🤝‍🧑🏽". They are in the same order like in the definition of the emoji (`Sequence`).
* `SkinTone[] NoSkinTones`: If the emoji does not has a specific skin tone (aka. yellow) `SkinTones` points to this list.
* `Group Group`: The [`Group`](unicode/Group.cs) of the emoji. Example: `SMILEYS_AND_EMOTION` for "😀" and `PEOPLE_AND_BODY` for "👨🏿‍⚕️"
* `string Subgroup`: The subgroup name of the emoji. Example: `"face-smiling"` for "😀" and `"person-role"` for "👨🏿‍⚕️"
* `bool HasGlyph`: Is there a glyph for the emoji defined in the `Segoe UI Emoji` font.
* `int SortOrder`: an integer indicating the place of this `SingleEmoji` instance in the recommended emoji order, per the Unicode Consortium. The emoji lists `Emoji.Basic` and `Emoji.All` are ordered by this value.
