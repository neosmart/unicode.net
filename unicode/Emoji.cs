using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace NeoSmart.Unicode
{
    //based off of https://github.com/iamcal/emoji-data
    //in particular, https://raw.githubusercontent.com/iamcal/emoji-data/master/emoji_pretty.json
    public class Emoji
    {
        public string Name { get; set; }
        public UnicodeSequence Unified { get; set; }
        public UnicodeSequence[] Variations { get; set; }
        public string ShortName { get; set; }
        public string[] ShortNames { get; set; }
        public string Text { get; set; }
        public string[] Texts { get; set; }
        public string Category { get; set; }
        public UnicodeSequence[] SkinVariations { get; set; }

        //Used by Newtonsoft to generate an Emoji object from the JSON data
        //the emoji-data project has a very bad json format where skin_variations are _members_ not array values,
        //so each and every emoji object has a different format! We are therefore forced to use dynamic here
        [JsonConstructor]
        public Emoji(string name, string unified, string[] variations, string short_name, string[] short_names, string text, string[] texts, string category, JObject skin_variations)
        {
            Name = name;
            Unified = new UnicodeSequence(unified.Replace('-', ','));
            Variations = variations.Select(x => new UnicodeSequence(x.Replace('-', ','))).ToArray();
            ShortName = short_name;
            ShortNames = short_names;
            Text = text;
            Texts = texts;
            Category = category;

            if (skin_variations != null) {
                SkinVariations = new UnicodeSequence[skin_variations.Count];
                int i = 0;
                foreach (var sv in skin_variations)
                {
                    SkinVariations[i++] = new UnicodeSequence(sv.Key);
                }
            }
        }
    }
}
