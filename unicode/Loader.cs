using System;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace NeoSmart.Unicode
{
    public class Loader
    {
        public static string CacheFolder;
        public List<Emoji> Emoji { get; private set; }
        public SortedSet<string> AllSequences { get; private set; }
        public SortedSet<string> Blacklist { get; private set; } = new SortedSet<string>()
        {
            "￼", //embedded object placeholder
        };
        public SortedSet<string> SkinTones { get; private set; } = new SortedSet<string>()
        {
            "🏻",
            "🏼",
            "🏽",
            "🏾",
            "🏿"
        };
        public UnicodeSequence VariationSelector { get; private set; } = new UnicodeSequence("FE0F");
        public UnicodeSequence ZeroWidthJoiner { get; private set; } = new UnicodeSequence("200D");

        public bool Loaded { get; private set; }

        public async Task<List<Emoji>> Download(bool forceRefresh = false)
        {
            if (Loaded)
            {
                return Emoji;
            }

            if (CacheFolder == null)
            {
                CacheFolder = "";
            }

            var path = Path.Combine(CacheFolder, "emoji.json");
            if (forceRefresh || !File.Exists(path))
            {
                using (var client = new HttpClient())
                {
                    var uri = new Uri("https://raw.githubusercontent.com/iamcal/emoji-data/master/emoji_pretty.json");
                    var j = await client.GetStringAsync(uri);
                    File.WriteAllText(path, j);
                }
            }

            var json = File.ReadAllText(path);
            Emoji = JsonConvert.DeserializeObject<List<Emoji>>(json);
            AllSequences = new SortedSet<string>();
            foreach (var emoji in Emoji)
            {
                AllSequences.Add(Encoding.Unicode.GetString(emoji.Unified.AsUtf16Bytes.ToArray()));
                foreach (var variation in emoji.Variations)
                {
                    AllSequences.Add(Encoding.Unicode.GetString(variation.AsUtf16Bytes.ToArray()));
                }
                foreach (var skinVariation in emoji.Variations)
                {
                    AllSequences.Add(Encoding.Unicode.GetString(skinVariation.AsUtf16Bytes.ToArray()));
                }
            }

            Loaded = true;
            return Emoji;
        }
    }
}
