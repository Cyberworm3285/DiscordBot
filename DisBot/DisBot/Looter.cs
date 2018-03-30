using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using System.IO;
using System.Linq;

using Loot4Standard.Looting;

namespace DisBot
{
    public static class Looter
    {
        private static LootTable<string> _loot;
        private static List<KeyValuePair<int, string>> _base;
        private static bool _initialized = false;

        public static string Next
        {
            get
            {
                Last = (_loot.Length>0)?_loot.Next(): "Nix Da :confused:";
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
                _base = JsonConvert.DeserializeObject<List<KeyValuePair<int, string>>>(File.ReadAllText(Config.Current.LootLocation));
                _loot = new LootTable<string>(_base);
                return true;
            }
            catch
            {
                _base = new List<KeyValuePair<int, string>>
                {
                    new KeyValuePair<int, string> (1000, "http://bc01.rp-online.de/polopoly_fs/63-millionen-rtl-zuschauschuldnerberatpetzwegat-familien-finanzklemme-1.503632.1315967723!httpImage/587917878.jpg_gen/derivatives/dx510/587917878.jpg")
                };
                _loot = new LootTable<string>(_base);
                return false;
            }
        }

        private static void UpdateURLs()
        {
            File.WriteAllText(Config.Current.LootLocation, JsonConvert.SerializeObject(_base, Formatting.Indented));
        }

        public static void AddURL(string url, int rarity)
        {
            rarity = (rarity < 1) ? 1 : (rarity > 1000) ? 1000 : rarity;
            _base.Add(new KeyValuePair<int, string>(rarity, url));
            _loot = new LootTable<string>(_base);
            UpdateURLs();
        }

        public static int Delete(params string[] values)
        {
            int c = _base.RemoveAll(x => values.Contains(x.Value));

            _loot = new LootTable<string>(_base);
            UpdateURLs();
            return c;
        }

        public static void Flush()
        {
            _base = new List<KeyValuePair<int, string>>();
            _loot = new LootTable<string>();
        }

        public static string GetHTMLFormattedOverview() =>
            $"<html>\n\t<body>\n{string.Join("\n",_loot.Select(x => $"\t\t<p><h1 style=\"font - family:verdana;\">{x.Key}</h1></p>\n\t\t<p><img src=\"{x.Value}\" alt=\"{x.Value}\"></p>"))}\n\t</body>\n</html>";
    }
}
