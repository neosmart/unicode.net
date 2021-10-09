const skipPunctuation = /[,.'’“”!]/g;
const spacePunctuation = /[()]/g;

let utr: number;
let unicodeVersion: string;

const intro = () => `namespace NeoSmart.Unicode
{
    // This file is machine-generated from the official Unicode Consortium UTR${utr} emoji
    // list found in Unicode ${unicodeVersion}. See the \`importers\` folder for the generator.
`;

const extro = () => `
}`;

function CamelCase(input: string, withSpaces = false) {
    if (input == null) {
        return "";
    }

    if (input.includes("&")) {
        input = input.replace("&", "and");
    }
    if (input.includes("#")) {
        input = input.replace("#", "Hash");
    }
    if (input.includes("*")) {
        input = input.replace("*", "Asterisk");
    }
    if (skipPunctuation.test(input)) {
        input = input.replace(skipPunctuation, "");
    }
    if (spacePunctuation.test(input)) {
        input = input.replace(spacePunctuation, " ");
    }
    const firstRegex = /\b1st/;
    if (firstRegex.test(input)) {
        input = input.replace(firstRegex, "First");
    }
    const secondRegex = /\b2nd/;
    if (secondRegex.test(input)) {
        input = input.replace(secondRegex, "Second");
    }
    const thirdRegex = /\b3rd/;
    if (thirdRegex.test(input)) {
        input = input.replace(thirdRegex, "Third");
    }

    let result = new Array(input.length);
    let capitalize = true;
    for (const c of input) {
        if (c == " " || c == "-" || c == "_" || c == ':') {
            capitalize = true;
            if (withSpaces) {
                result.push(" ");
            }
            continue;
        }
        if (capitalize) {
            result.push(c.toUpperCase());
            capitalize = false;
        } else {
            result.push(c.toLowerCase());
        }
    }

    return result.join("");
}

function getBinary(path: string) {
    return new Promise(function(resolve, reject) {
        var xhr = new XMLHttpRequest();
        xhr.overrideMimeType("application/octet-stream");
        xhr.open('GET', path, true);
        xhr.responseType = 'arraybuffer';

        xhr.onload = function(e) {
            if (this.status != 200) {
                reject(e);
                return;
            }

            resolve(this.response);
        }

        xhr.send();
    });
}

function getText(path: string) {
    return new Promise(function(resolve, reject) {
        var xhr = new XMLHttpRequest();
        xhr.overrideMimeType("text/plain");
        xhr.open('GET', path, true);
        xhr.responseType = 'text';

        xhr.onload = function(e) {
            if (this.status != 200) {
                reject(e);
            } else {
                resolve(this.responseText);
            }
        };

        xhr.send();
    });
}

function makeStringArray(keywords: string) {
    return keywords
        .split(/[ \-_:]/)
        .map(word => word.replace(skipPunctuation, ""))
        .map(word => word.replace(spacePunctuation, ""))
        .filter(word => word.length > 0)
        .map(word => word.toLowerCase())
        .filter(word => !/^(of|with|without|and|or|&|on|the|in)$/.test(word))
        .map(word => `"${word}"`)
        .join(", ");
}

function makeSortedSet(name: string, emoji: Array<Emoji>, summary = ""): string {
    let result = `using System.Collections.Generic;

${intro()}
    public static partial class Emoji
    {` + (summary.length === 0 ? "" : `
        /// <summary>
        /// ${summary}
        /// </summary>`) + `
#if NET20 || NET30 || NET35
        public static List<SingleEmoji> ${name}
#else
        public static SortedSet<SingleEmoji> ${name}
#endif
        {
            get
            {
                if (_${name} == null)
                {
                    _${name} = Generate${name}();
                }
                return _${name};
            }
        }
#if NET20 || NET30 || NET35
        private static List<SingleEmoji> _${name};
        private static List<SingleEmoji> Generate${name}() => new List<SingleEmoji>()
#else
        private static SortedSet<SingleEmoji> _${name};
        private static SortedSet<SingleEmoji> Generate${name}() => new SortedSet<SingleEmoji>()
#endif
        {
`;

    for (const e of emoji) {
        result += `            /* ${e.symbol} */ ${CamelCase(e.name)},
`;
    }
    result += `        };
    }`;

    result += extro();

    return result;
}

function isBasicEmoji(emoji: Emoji) {
    return !emoji.name.match(/skin tone|keycap/i);
}

// Segoe UI reuses the same symbol when an ungendered version of the emoji is also
// available. e.g. ConstructionWorker and ManConstructionWorker are the same.
function isGenderedDuplicate(deduplicator: Set<string>, emoji: Emoji) {
    const regex = /^(man|woman)\s/i;

    if (emoji.name.match(regex)) {
        if (deduplicator.has(emoji.name.replace(regex, ""))) {
            // e.g. ManPoliceOfficer/WomanPoliceOffer -> PoliceOfficer
            return true;
        }
        if (deduplicator.has(emoji.name.replace(regex, "person "))) {
            // e.g. ManPouting/WomanPouting -> PersonPouting
            return true;
        }
        return false;
    }

    deduplicator.add(emoji.name);
    return false;
}

function emojiToCSharp(emoji: Emoji) {
    return `
        /* ${emoji.symbol} */
        public static SingleEmoji ${CamelCase(emoji.name)} => new SingleEmoji(
                sequence: new UnicodeSequence(${emoji.sequence.map(s => `0x${s}`).join(", ")}),
                name: "${emoji.name}",
                group: "${emoji.group}",
                subgroup: "${emoji.subgroup}",
                searchTerms: new [] { ${makeStringArray(emoji.name)} },
                sortOrder: ${emoji.index}
            );
`;
}

function fontSupportsEmoji(font: Font, emoji: Emoji) {
    const menWomenRegex = /\b(men|women)\b/;

    // Hard-coded elimination: glyphs for Men* and Women* are rendered as the basic
    // ungendered emoji in Segoe UI Emoji, but with the gender icon tacked on after.
    if (menWomenRegex.test(emoji.name)) {
        return false;
    }

    // We consider an emoji to be "supported" if it has a glyph in the COLR table, that
    // glyph has an id that is not 0, and it is rendered as a single glyph.
    let layout = font.layout(emoji.symbol);
    if (layout.glyphs.length != 1) {
        // This is not being rendered as a single emoji, which means it's not actually
        // supported.
        return false;
    }
    if (layout.glyphs[0].id == 0) {
        // This would be rendered as a placeholder for a missing glyph, so also not
        // supported.
        return false;
    }

    // All clear!
    return true;
}

interface Emoji {
    sequence: string[],
    symbol: string,
    name: string,
    index: number,
    group: string,
    subgroup: string,
}

function* parse(data: string) /* : IterableIterator<Emoji> */ {
    const parser = /(.*?)\s+;.*# (\S+) (?:E[0-9.]+ ?)(.*)/;
    const lines = data.split("\n");
    const groupRegex = /\bgroup: \s*(\S.+?)\s*$/;
    const subgroupRegex = /subgroup: \s*(\S.+?)\s*$/;
    const utrRegex = /\/tr(\d+)\b/;
    const unicodeRegex = /Version: (\d+.\d+)/i;

    let deduplicator = new Set();
    let group = "";
    let subgroup = "";
    let sortIndex = 0;
    for (let i = 0; i < lines.length; ++i) {
        const line = lines[i];
        if (line.startsWith("#") || !line.includes("fully-qualified")) {
            let match;
            if (match = line.match(groupRegex)) {
                group = (match[1]);
            } else if (match = line.match(subgroupRegex)) {
                subgroup = (match[1]);
            } else if (utr === undefined && (match = line.match(utrRegex))) {
                utr = parseInt(match[1]);
            } else if (unicodeVersion === undefined && (match = line.match(unicodeRegex))) {
                unicodeVersion = match[1];
            }
            continue;
        }

        let results = line.match(parser);

        if (results == null) {
            console.log(`Error parsing emoji. Line ${i}: `, line);
            continue;
        }

        const emoji: Emoji = {
            "sequence": results[1].split(" "),
            "symbol": results[2],
            "name": results[3],
            "index": sortIndex++,
            "group": group,
            "subgroup": subgroup,
        };

        if (deduplicator.has(emoji.name)) {
            continue;
        }

        let oldName = emoji.name;
        let version = 2;
        while (deduplicator.has(CamelCase(emoji.name))) {
            emoji.name = oldName + version++;
        }
        deduplicator.add(emoji.name);
        deduplicator.add(CamelCase(emoji.name));

        yield emoji;
    }
}

function parseEmoji(data: string) {
    return new Lazy(parse(data));
}

class CodeResult {
    emoji: string = "";
    lists = {
        all: "",
        basic: "",
    };
    range: string = "";
}

class CodeGenerator {
    private font: Font;
    private data: string;

    constructor(font: Font, data: string) {
        this.font = font;
        this.data = data;
    }

    generate(): CodeResult {
        // Parse data and generate actual emoji
        let emoji = Array.from(parseEmoji(this.data));

        let csharp = new CodeResult();

        // Dump actual emoji objects. All other operations print only references to these.
        csharp.emoji = this.emoji(emoji);

        // Generate a C# list of all emoji.
        csharp.lists.all = makeSortedSet("All", emoji);

        // Generate a subset of emoji that is ungendered and without skintone, further
        // restricted to only emoji supported by the version of Segoe UI Emoji that we are
        // targeting.
        csharp.lists.basic = this.basicEmoji(emoji);

        // Generate a C# list of ranges that constitute (or can constitute, if used with a
        // VS codepoint) an emoji.
        csharp.range = this.range(emoji);

        return csharp;
    }

    private emoji(emoji: Emoji[]): string {
        let code = [];
        code.push(intro());
        code.push("    public static partial class Emoji\n");
        code.push("    {");
        for (const e of emoji) {
            code.push(emojiToCSharp(e));
        }
        code.push("    }");
        code.push(extro());

        return code.join("");
    }

    private basicEmoji(emoji: Emoji[]): string {
        // Segoe UI duplicates symbols when emoji is available as both ungendered and
        // gendered.
        let deduplicator = new Set<string>();
        let supportedEmoji = emoji
            .filter(isBasicEmoji)
            .filter(e => !isGenderedDuplicate(deduplicator, e))
            .filter(e => fontSupportsEmoji(this.font, e))

        // Dump list of ungendered emoji
        return makeSortedSet("Basic", supportedEmoji,
            "A (sorted) enumeration of all emoji without skin variations and no duplicate " +
            "gendered vs gender-neutral emoji, ideal for displaying. " +
            "Emoji without supported glyphs in Segoe UI Emoji are also omitted from this list.");
    }

    private range(emoji: Emoji[]): string {
        // First create a deduplicated set of all codepoints found across all the emoji.
        const codepoints = new Set<number>();
        for (const e of emoji) {
            for (const s of e.sequence) {
                const val = parseInt(s, 16);
                codepoints.add(val);
            }
        }

        // Then export it as a sorted list so we can enumerate in-order

        // let sorted = Array.from(codepoints.values());
        // sorted = sorted.sort();

        // Javascript is a brain-dead language and I don't know why we are using it here.
        // The stupid thing defaults to sorting numbers alphabetically rather than
        // naturally!

        let sorted = new Int32Array(codepoints.values());
        sorted = sorted.sort();

        const format = function(n: number): string {
            return n.toString(16).padStart(4, '0').toUpperCase();
        }

        let ranges: CodepointRange[] = [];
        let start = 0;
        let prev = 0;
        sorted.forEach((cp, i) => {
            if (cp <= prev) {
                throw new Error(`enumerating out of order: ${format(cp)} came after ${format(prev)}`);
            }
            if (cp != prev + 1 || i == sorted.length - 1) {
                if (start != 0) {
                    ranges.push(new CodepointRange(start, prev));
                }
                start = cp;
            }
            prev = cp;
        });

        let code = [];
        code.push(intro());
        code.push("    public partial class Languages");
        code.push("    {");
        code.push("        public static MultiRange Emoji = new MultiRange(");
        for (const range of ranges) {
            // code.push(`            Range(${range.begin}, ${range.end}),`);
            if (range.begin == range.end) {
                code.push(`            "${format(range.begin)}",`);
            } else {
                code.push(`            "${format(range.begin)}..${format(range.end)}",`);
            }
        }
        code[code.length - 1] = code[code.length - 1].replace(',', "");
        code.push("        );");
        code.push("    }");
        code.push(extro());

        return code.join("\n");
    }
}

class CodepointRange {
    begin: number;
    end: number;

    constructor(begin: number, end: number) {
        this.begin = begin;
        this.end = end;
    }
}

var module: any;
if (module == undefined) {
    module = {};
}

module.exports = {
    CodeGenerator: CodeGenerator,
};
