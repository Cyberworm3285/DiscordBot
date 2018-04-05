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
    [Serializable]
    public class ShortCuts : Dictionary<string, string>
    {
        public ShortCuts() : base(StringComparer.CurrentCultureIgnoreCase) { }
    }

    public static class Looter
    {
        private static LootTable<Meme> _loot;
        private static List<KeyValuePair<int, Meme>> _base;
        private static ShortCuts _shortcuts;

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

        public static IEnumerable<string> Shorts => _shortcuts.Select(x => x.Key);

        public static void Init()
        {
            if (_initialized)
                return;
            else
                _initialized = true;

            TryLoadURLs();
            TryLoadShortcuts();
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
                    new KeyValuePair<int, Meme> (500, new Meme("http://bc01.rp-online.de/polopoly_fs/63-millionen-rtl-zuschauschuldnerberatpetzwegat-familien-finanzklemme-1.503632.1315967723!httpImage/587917878.jpg_gen/derivatives/dx510/587917878.jpg", "Dummmy", Math.Round(Math.PI, 6).ToString()))
                };
                _loot = new LootTable<Meme>(_base);
                return false;
            }
        }

        private static void UpdateURLs()
        {
            File.WriteAllText(Config.Current.LootLocation, JsonConvert.SerializeObject(_base, Formatting.Indented));
        }

        private static bool TryLoadShortcuts()
        {
            try
            {
                _shortcuts = JsonConvert.DeserializeObject<ShortCuts>(File.ReadAllText(Config.Current.ShortcutLocation));
                return true;
            }
            catch
            {
                _shortcuts = new ShortCuts();
                return false;
            }
        }

        private static void UpdateShortcuts()
        {
            File.WriteAllText(Config.Current.ShortcutLocation, JsonConvert.SerializeObject(_shortcuts, Formatting.Indented));
        }

        public static string ForceMeme(int index) => _base[index].Value.URL;

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
            if (
                (!exists 
                || !new[] { ".png", ".jpg", "gif", ".gifv", ".mp4" }.Any(x => url.EndsWith(x, StringComparison.InvariantCultureIgnoreCase))) 
                && !url.StartsWith("https://www.youtube.com/watch?")
                )
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
                $"<html>"
                + "\n\t\t<style>"
                + "\n\t* {font-family: Arial; font-size: 16px;}"
                + "\n\t.main-container {display: flex;flex-wrap: wrap; }"
                + "\n\t.item-container {  flex-grow: 1; margin: 0 5px 5px 0; border: 0px solid black;}"
                + "\n\t.item-container .top-row { display: flex; box-sizing: border-box;}"
                + "\n\t.top-row div {padding: 10px; border-right: 0px solid black; flex-grow: 1;}"
                + "\n\t.top-row div:nth-child(1) {background-color: #FDE74C;}"
                + "\n\t.top-row div:nth-child(2) {background-color: #5BC0EB;}"
                + "\n\t.top-row div:nth-child(3) {background-color: #9BC53D;}"
                + "\n\t.top-row div:nth-last-child(1) {border-right: 0px solid black; background-color: #FC7753; }"
                + "\n\t.item-container .image{padding-top:56.29%;background-size:cover;background-repeat:no-repeat;}"
                + "\n\t</style>"
                + "\n\t<body>"
                + "\n\t\t<div class='main-container'>"
                + string.Join("\n", 
                    _loot.Select(x => 
                    {
                        string s = $"\t\t\t<div class='item-container'><div class='top-row'><div>{c++}</div><div>{x.Key - sub}</div><div>{x.Value.Username}</div><div>{x.Value.ID}</div></div><div class='image' style='background-image: url({x.Value.URL})'></div></div>";
                        sub = x.Key;
                        return s;
                    })
                )
                + "\n\t</div>"
                + "\n\t</body>\n</html>";
        }

        public static bool Contains(string s) => _loot.Any(x => x.Value.URL == s);

        public static bool AddShortcut(string s, int index)
        {
            if (_shortcuts.TryAdd(s, _base[index].Value.URL))
            {
                UpdateShortcuts();
                return false;
            }
            _shortcuts.Remove(s);
            _shortcuts.Add(s, _base[index].Value.URL);
            UpdateShortcuts();
            return true;

        }

        public static bool DeleteShortcut(string s) =>
            _shortcuts.Remove(s);

        public static (string url, bool exists) ProcessShortcut(string s)
        {
            string res = null;
            if (_shortcuts.TryGetValue(s, out res))
                return (res, true);
            else
                return (null, false);
        }
    }
}
