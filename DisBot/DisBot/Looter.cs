using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using System.IO;
using System.Linq;

using Loot4Standard.Looting;
using System.Net;

namespace DisBot
{
    public static class Looter
    {
        private static LootTable<Meme> _loot;
        private static List<KeyValuePair<int, Meme>> _base;
        private static bool _initialized = false;

        public static string Next
        {
            get
            {
                Last = (_loot.Length>0)?_loot.Next().URL: "Nix Da :confused:";
                return Last;
            }
        }
        public static string Last { get; private set; }

        public static void Init()
        {
            if (_initialized)
                return;
            else
                _initialized = true;

            TryLoadURLs();
            Console.WriteLine("Looter initialized");
        }

        private static bool TryLoadURLs()
        {
            try
            {
                _base = JsonConvert.DeserializeObject<List<KeyValuePair<int, Meme>>>(File.ReadAllText(Config.Current.LootLocation));
                _loot = new LootTable<Meme>(_base);
                return true;
            }
            catch
            {
                _base = new List<KeyValuePair<int, Meme>>
                {
                    new KeyValuePair<int, Meme> (1000, new Meme("http://bc01.rp-online.de/polopoly_fs/63-millionen-rtl-zuschauschuldnerberatpetzwegat-familien-finanzklemme-1.503632.1315967723!httpImage/587917878.jpg_gen/derivatives/dx510/587917878.jpg", "Dummmy", Math.Round(Math.PI, 6).ToString()))
                };
                _loot = new LootTable<Meme>(_base);
                return false;
            }
        }

        private static void UpdateURLs()
        {
            File.WriteAllText(Config.Current.LootLocation, JsonConvert.SerializeObject(_base, Formatting.Indented));
        }

        public static bool AddURL(string url, int rarity, string username, string id)
        {
            bool exists;
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = "HEAD";
                request.GetResponse();
                exists = true;
            }
            catch
            {
                exists = false;
            }
            if (!exists || !new[] { ".png", ".jpg", "gif" }.Any(x => url.EndsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                return false;
            rarity = (rarity < 1) ? 1 : (rarity > 1000) ? 1000 : rarity;
            _base.Add(new KeyValuePair<int, Meme>(rarity, new Meme(url, username, id)));
            _loot = new LootTable<Meme>(_base);
            UpdateURLs();
            return true;
        }

        public static int Delete(string value)
        {
            int c = _base.RemoveAll(x => value.Contains(x.Value.URL));

            _loot = new LootTable<Meme>(_base);
            UpdateURLs();
            return c;
        }

        public static bool Delete(int index)
        {
            bool res = false;
            try
            {
                _base.RemoveAt(index);
                res = true;
            }
            catch
            {
                _loot = new LootTable<Meme>(_base);
            }
            UpdateURLs();
            return res;
        }

        public static void Flush()
        {
            _base = new List<KeyValuePair<int, Meme>>();
            _loot = new LootTable<Meme>();
        }

        public static string GetHTMLFormattedOverview()
        {
            int c = 0, sub = 0;
            return 
                $"<html>\n\t<body>\n"
                +string.Join("\n", 
                    _loot.Select(x => 
                    {
                        string s = $"\t\t<p><h1 style=\"font-family:verdana;\">{c++} :: {x.Key - sub} submitted by {x.Value.Username} ({x.Value.ID})</h1></p>\n\t\t<p><img src=\"{x.Value.URL}\" alt=\"{x.Value.URL}\" style=\"max-height: 300px;\"></p>";
                        sub = x.Key;
                        return s;
                    })
                )
                +"\n\t</body>\n</html>";
        }

        public static bool Contains(string s) => _loot.Any(x => x.Value.URL == s);
    }
}
