const skipPunctuation = /[,.'’“”!]/g;
const spacePunctuation = /[()]/g;
const firstRegex = /\b1st/;
const secondRegex = /\b2nd/;
const thirdRegex = /\b3rd/;

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
    return Enumerable
        .From(keywords.split(" "))
        .Where(x => x != "")
        .SelectMany(x => x.split("_"))
        .SelectMany(x => x.split("-"))
        .SelectMany(x => x.split(":"))
        .Select(x => x.trim().toLowerCase())
        .Where(x => !(["", "of", "with", "without", "and", "or", "&", "on", "the", "in"]
            .includes(x))).Select(x => sprintf('"%s"', x))
        .Distinct()
        .ToArray()
        .join(", ");
}

function makeSortedSet(name, emoji) {
    result = sprintf('public static readonly SortedSet<SingleEmoji> %s = new SortedSet<SingleEmoji>() {\n', name);
    emoji.each(e => result += sprintf('\t/* %s */ %s,\n', e.symbol, CamelCase(e.name)));
    result += '};';

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
    return sprintf(
        "public static readonly SingleEmoji %s = new SingleEmoji( \n\
sequence: new UnicodeSequence(\"%s\"),\n\
name: \"%s\",\n\
searchTerms: new [] { %s },\n\
sortOrder: %d\n\
);\n\
", CamelCase(emoji.name), emoji.sequence, emoji.name, makeStringArray(emoji.name), emoji.index);
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

function parseEmoji(data) {
    var lines = data.split("\n");
    var i = 0;
    return Lazy(lines).filter(l => !l.startsWith("#") && l.includes("fully-qualified")).map(function(l) {
        var results = l.match(/(.*?)\s+;.*# (\S+) (.*)/);
        var emoji = {
            "sequence": results[1],
            "symbol": results[2],
            "name": results[3],
            "index": i++
        };
        // console.log(emoji);
        return emoji;
    }).uniq(x => CamelCase(x.name));
}

const manWomanRegex = /^(man|woman)/i;
const manWomanRegexPlus = /^(man|woman) */i;

function ungenderedEmoji(emoji) {
    // Yes, this is insanely slow and could be optimized by making a trimmed & sorted
    // second/third list and taking the distinct union, but who cares?
    return emoji.filter(e => !manWomanRegex.test(e.name)
        || !emoji.some(e2 => (e2.name.toLowerCase() == e.name.replace(manWomanRegex, "person").toLowerCase()
            || e2.name.toLowerCase() == e.name.replace(manWomanRegexPlus, "").toLowerCase())
    ));
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

        let emoji = parseEmoji(this.responseText); // all emoji
        let basicEmoji = emoji.filter(isBasicEmoji); // emoji suitable for display
        let supportedEmoji = basicEmoji.filter(e => fontSupportsEmoji(font, e)); // emoji supported by Segoe UI Emoji

        // dump all emoji list
        // makeSortedSet(emoji);

        // dump basic emoji list
        let basicUngenderedEmoji = ungenderedEmoji(basicEmoji);
        makeSortedSet("Basic", basicUngenderedEmoji);
    };

    xhr.send();
});
