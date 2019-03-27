all: unicode/Emoji-All.cs unicode/Emoji-Basic.cs unicode/Emoji-Emojis.cs

importers/importer.js: importers/*.ts
	cd importers; tsc

unicode/Emoji-All.cs unicode/Emoji-Basic.cs unicode/Emoji-Emojis.cs: importers/importer.js importers/index.js importers/emoji-test.txt
	cd importers; node index.js

