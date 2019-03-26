const fs = require("fs").promises;
const path = require("path");
const fontkit = require("fontkit");
const importer = require("./importer.js");

const codeRoot = "../unicode";
(async function() {
    let text = await fs.readFile("emoji-test.txt", { encoding: "utf-8" });
    let font = fontkit.create(await fs.readFile("seguiemj.ttf"));

    let generator = new importer.CodeGenerator(font, text);

    let code = generator.generate();

    await fs.writeFile(codeRoot + "/Emoji-Emojis.cs", code.emoji, { encoding: "utf-8" });
    await fs.writeFile(codeRoot + "/Emoji-All.cs", code.lists.all, { encoding: "utf-8" });
    await fs.writeFile(codeRoot + "/Emoji-Basic.cs", code.lists.basic, { encoding: "utf-8" });
})();
