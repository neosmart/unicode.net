CONFIG = "Debug"
LIBFX = "netstandard2.0"
TESTFX = "netcoreapp2.2"

all: unicode/Emoji-All.cs unicode/Emoji-Basic.cs unicode/Emoji-Emojis.cs

importers/importer.js: importers/*.ts
	cd importers; tsc

unicode/Emoji-All.cs unicode/Emoji-Basic.cs unicode/Emoji-Emojis.cs: importers/importer.js importers/index.js importers/emoji-test.txt
	cd importers; node index.js

unicode/bin/$(CONFIG)/netstandard2.0/NeoSmart.Unicode.dll: unicode/*.cs
	dotnet build -f $(LIBFX) unicode/

.PHONY: test
test: unicode/bin/$(CONFIG)/netstandard2.0/NeoSmart.Unicode.dll
	dotnet test -f $(TESTFX) tests/
