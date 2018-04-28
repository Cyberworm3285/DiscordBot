using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;
using Newtonsoft.Json;

namespace DisBot.Memes
{
    public enum MemeType
    {
        Embed,
        LinkOnly
    }

    public class Meme
    {
        public string URL { get; set; }
        public string Username { get; set; }
        public string ID { get; set; }
        public MemeType Type { get; set; }
        public HashSet<string> Tags { get; set; } = new HashSet<string> { };

        public Meme(string url, string usermane, string id, MemeType type, params string[] tags)
        {
            URL = url;
            Username = usermane;
            ID = id;
            Type = type;

            if (tags != null || tags.Length > 0)
            {
                Array.ForEach(tags, x => Tags.Add(x));
            }
        }

        [JsonConstructor]
        private Meme() { }

        public override string ToString()
            => $"[{Username}||{ID}]->{Type}";
    }
}
