const skipPunctuation = /[,.'’“”!]/g;
const spacePunctuation = /[()]/g;
const firstRegex = /\b1st/;
const secondRegex = /\b2nd/;
const thirdRegex = /\b3rd/;

// Implementation of Lazy derived from the code at
// https://dev.to/nestedsoftware/lazy-evaluation-in-javascript-with-generators-map-filter-and-reduce--36h5
class Lazy {
    constructor(iterable, callback) {
        this.iterable = iterable
        this.callback = callback
    }

    filter(callback) {
        return new LazyFilter(this, callback)
    }

    map(callback) {
        return new LazyMap(this, callback)
    }

    [Symbol.iterator]() {
        return {
            next: () => {
                return this.iterable.next();
            }
        }
    }

    next() {
        return this.iterable.next()
    }

    take(n) {
        const values = []
        for (let i = 0; i < n; i++) {
            values.push(this.next().value)
        }

        return values
    }
}

class LazyFilter extends Lazy {
    next() {
        while (true) {
            const item = this.iterable.next()

            if (this.callback(item.value)) {
                return item
            }
        }
    }
}

class LazyMap extends Lazy {
    next() {
        const item = this.iterable.next()

        const mappedValue = this.callback(item.value)
        return {
            value: mappedValue,
            done: item.done
        }
    }
}

function CamelCase(string, withSpaces = false) {
    if (string == null) {
        return "";
    }

    if (string.includes("&")) {
        string = string.replace("&", "and");
    }
    if (string.includes("#")) {
        string = string.replace("#", "Hash");
    }
    if (string.includes("*")) {
        string = string.replace("*", "Asterisk");
    }
    if (skipPunctuation.test(string)) {
        string = string.replace(skipPunctuation, "");
    }
    if (spacePunctuation.test(string)) {
        string = string.replace(spacePunctuation, " ");
    }
    if (firstRegex.test(string)) {
        string = string.replace(firstRegex, "First");
    }
    if (secondRegex.test(string)) {
        string = string.replace(secondRegex, "Second");
    }
    if (thirdRegex.test(string)) {
        string = string.replace(thirdRegex, "Third");
    }

    let result = new Array(string.length);
    let capitalize = true;
    for (const c of string) {
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

function makeStringArray(keywords) {
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

function makeSortedSet(name, emoji) {
    result = `public static readonly SortedSet<SingleEmoji> ${name} = new SortedSet<SingleEmoji>() {
`;

    for (const e of emoji) {
        result += `\t/* ${e.symbol} */ ${CamelCase(e.name)},
`;
    }
    result += `};

`;

    $("#results").append(document.createTextNode(result))
}

function isBasicEmoji(emoji) {
    return !emoji.name.match(/skin tone|keycap/i);
}

function isGeneredEmoji(emoji) {
    return !emoji.name.startsWith("person");
}

function isUngenderedEmoji(emoji) {
    return !emoji.name.startsWith("man") && !emoji.name.startsWith("woman");
}

function emojiToCSharp(emoji) {
    return `/* ${emoji.symbol} */
public static readonly SingleEmoji ${CamelCase(emoji.name)} = new SingleEmoji(
        sequence: new UnicodeSequence("${emoji.sequence}"),
        name: "${emoji.name}",
        group: "${emoji.group}",
        subgroup: "${emoji.subgroup}",
        searchTerms: new [] { ${makeStringArray(emoji.name)} },
        sortOrder: ${emoji.index},
    );

`;
}

function loadFont(fontPath, callback) {
    var xhr = new XMLHttpRequest();
    xhr.overrideMimeType("text/plain");
    xhr.open('GET', fontPath, true);
    xhr.responseType = 'arraybuffer';

    xhr.onload = function(e) {
        if (this.status != 200) {
            console.log("Load font failed!");
            console.log(e);
            return;
        }

        var fkBlob = this.response;
        var fkBuffer = new Buffer(fkBlob);
        var font = fontkit.create(fkBuffer);
        callback(font);
    }
    xhr.send();
}

const menWomenRegex = /\b(men|women)\b/;

function fontSupportsEmoji(font, emoji) {
    // Hard-coded elimination: glyphs for Men* and Women* are the basic ungendered emoji
    // in Segoe UI Emoji, but with the gender icon tacked on.
    if (menWomenRegex.test(emoji.name)) {
        return false;
    }

    // We consider an emoji to be "supported" if it has a glyph in the COLR table, that
    // glyph has an id that is not 0, and it is rendered as only one, single glyph.
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

function *parse(data) {
    const parser = /(.*?)\s+;.*# (\S+) (.*)/;
    const lines = data.split("\n");
    const groupRegex = /\bgroup: \s*(\S.+?)\s*$/;
    const subgroupRegex = /subgroup: \s*(\S.+?)\s*$/;

    let group = "";
    let subgroup = "";
    for (let i = 0; i < lines.length; ++i) {
        const line = lines[i];
        if (line.startsWith("#") || !line.includes("fully-qualified")) {
            if (match = line.match(groupRegex)) {
                group = (match[1]);
            } else if (match = line.match(subgroupRegex)) {
                subgroup = (match[1]);
            }
            continue;
        }

        let results = line.match(parser);

        yield {
            "sequence": results[1],
            "symbol": results[2],
            "name": results[3],
            "index": i++,
            "group": group,
            "subgroup": subgroup,
        };
    }
}

function parseEmoji(data) {
    return new Lazy(parse(data), () => true);
}

const manWomanRegex = /^(man|woman)/i;
const manWomanRegexPlus = /^(man|woman) */i;

// This should not be passed a generator but rather a full-on Array
function ungenderedEmoji(emoji) {
    // Yes, this is insanely slow and could be optimized by making a trimmed & sorted
    // second/third list and taking the distinct union, but who cares?
    return emoji.filter(e => !manWomanRegex.test(e.name) ||
        !emoji.some(e2 => (e2.name.toLowerCase() == e.name.replace(manWomanRegex, "person").toLowerCase() ||
            e2.name.toLowerCase() == e.name.replace(manWomanRegexPlus, "").toLowerCase())));
}

loadFont("./seguiemj.ttf", font => {
    // emoji-test.txt from http://unicode.org/Public/emoji/5.0/emoji-test.txt

    var xhr = new XMLHttpRequest();
    xhr.overrideMimeType("text/plain");
    xhr.open('GET', "emoji-test.txt", true);
    xhr.responseType = 'text';

    xhr.onload = function(e) {
        if (this.status != 200) {
            console.log("Load emoji-test.txt failed!");
            console.log(e);
            return;
        }

        let emoji = Array.from(parseEmoji(this.responseText)); // all emoji

        // Dump actual emoji objects. All other operations print only references to these.
        for (const e of emoji) {
            $("#results").append(document.createTextNode(emojiToCSharp(e)))
        }

        // Dump all emoji list
        makeSortedSet("All", emoji);

        // Some partial lists of emoji filtered by certain criteria
        let basicEmoji = emoji.filter(isBasicEmoji); // removes meta emoji
        let basicUngenderedEmoji = ungenderedEmoji(Array.from(basicEmoji));

        // Narrow it down to emoji supported by Segoe UI Emoji
        let supportedEmoji = basicUngenderedEmoji
            .filter(isBasicEmoji)
            .filter(e => fontSupportsEmoji(font, e));

        // Dump list of ungendered emoji
        makeSortedSet("Basic", supportedEmoji);
    };

    xhr.send();
});
