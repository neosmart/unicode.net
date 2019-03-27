const skipPunctuation = /[,.'’“”!]/g;
const spacePunctuation = /[()]/g;

let utr: number;
let unicodeVersion: string;

const intro = () => `namespace NeoSmart.Unicode
{
    // This file is machine-generated from the official Unicode Consortium UTR${utr} publication
    // detailing the emoji found in Unicode ${unicodeVersion}
    // See the \`importers\` folder for the generators.
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
        public static List<SingleEmoji> ${name} => new List<SingleEmoji>() {
#else
        public static SortedSet<SingleEmoji> ${name} => new SortedSet<SingleEmoji>() {
#endif
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

// Segoe UI reuses the same symbol when a ungendered version of the emoji is also available.
// e.g. ConstructionWorker and ManConstructionWorker are the same.
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
        public static readonly SingleEmoji ${CamelCase(emoji.name)} = new SingleEmoji(
                sequence: new UnicodeSequence(${emoji.sequence.map(s => `0x${s}`).join(", ")}),
                name: "${emoji.name}",
                group: "${emoji.group}",
                subgroup: "${emoji.subgroup}",
                searchTerms: new [] { ${makeStringArray(emoji.name)} },
                sortOrder: ${emoji.index}
            );
`;
}

const menWomenRegex = /\b(men|women)\b/;

function fontSupportsEmoji(font: Font, emoji: Emoji) {
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
    const parser = /(.*?)\s+;.*# (\S+) (.*)/;
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

        // Dump actual emoji objects.
        // All other operations print only references to these.
        let code = [];
        code.push(intro());
        code.push("    public static partial class Emoji\n");
        code.push("    {");
        for (const e of emoji) {
            code.push(emojiToCSharp(e));
        }
        code.push("    }");
        code.push(extro());
        csharp.emoji = code.join("");

        // Dump all emoji list
        csharp.lists.all = makeSortedSet("All", emoji);

        // Narrow it down to emoji supported by Segoe UI Emoji
        // Segoe UI duplicates symbols when emoji is available as both ungendered and gendered
        let deduplicator = new Set();
        let supportedEmoji = emoji
            .filter(isBasicEmoji)
            .filter(e => !isGenderedDuplicate(deduplicator, e))
            .filter(e => fontSupportsEmoji(this.font, e))

        // Dump list of ungendered emoji
        csharp.lists.basic = makeSortedSet("Basic", supportedEmoji,
            "A (sorted) enumeration of all emoji without skin variations and no duplicate " +
            "gendered vs gender-neutral emoji, ideal for displaying. " +
            "Emoji without supported glyphs in Segoe UI Emoji are also omitted from this list.");

        return csharp;
    }
}

var module: any;
if (module == undefined) {
    module = {};
}

module.exports = {
    CodeGenerator: CodeGenerator,
};
