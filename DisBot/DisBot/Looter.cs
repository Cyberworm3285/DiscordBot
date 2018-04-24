using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net;

namespace DisBot
{
    [Serializable]
    public class NCS_String_Dictionary : Dictionary<string, string>
    {
        public NCS_String_Dictionary() : base(StringComparer.CurrentCultureIgnoreCase) { }
    }

    public static class Looter
    {
        private static List<Meme> _base;
        private static NCS_String_Dictionary _shortcuts;
        private static bool _initialized = false;
        public static IEnumerable<string> Shorts => _shortcuts.Select(x => $"{x.Key}");
        public static int Count => _base.Count;

        #region Operations

        public static (Meme m, int index) Next => (_base.Count>0)?_base.Rand():(null, -1);

        public static Meme ForceMeme(int index) => _base[index];

        public static int IndexOf(string url) => _base.FindIndex(x => x.URL == url);


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

        public static int Delete(string value)
        {
            int c = _base.RemoveAll(x => value.Contains(x.URL));

            UpdateURLs();
            return c;
        }

        public static bool Delete(int index)
        {
            try
            {
                _base.RemoveAt(index);
                UpdateURLs();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Flush()
        {
            _base = new List<Meme>();
            _shortcuts = new NCS_String_Dictionary();
            UpdateURLs();
        }

        public static string GetHTMLFormattedOverview()
        {
            int c = 0;
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
                    _base.Select(x => 
                    {
                        string s = $"\t\t\t<div class='item-container'><div class='top-row'><div>{c++}</div><div>[Obsolet]</div><div>{x.Username}</div><div>{x.ID}</div></div><div class='image' style='background-image: url({x.URL})'></div></div>";
                        return s;
                    })
                )
                + "\n\t</div>"
                + "\n\t</body>\n</html>";
        }

        public static bool Contains(string s) => _base.Any(x => x.URL == s);

        public static Dictionary<string, int> GetStats()
        {
            Dictionary<string,int> stats = new Dictionary<string, int>();
            for (int i = 0; i < _base.Count; i++)
            {
                var m = _base[i];
                if (!stats.TryAdd(m.Username, 1))
                        stats[m.Username]++;
            }
            return stats;
        }

        #endregion

        #region URL

        private static MemeType CheckURL_Type(string url) =>
            (Config.Current.ExceptionPrefixes.Any(x => url.StartsWith(x)) || Config.Current.ExceptionSuffixes.Any(x => url.EndsWith(x)))
                ? MemeType.LinkOnly
                : MemeType.Embed;

        private static bool TryLoadURLs()
        {
            try
            {
                _base = JsonConvert.DeserializeObject<List<Meme>>(File.ReadAllText(Config.Current.LootLocation));
                ApplyMemeTypes();

                return true;
            }
            catch
            {
                try
                {
                    var temp = JsonConvert.DeserializeObject<List<KeyValuePair<int, Meme>>>(File.ReadAllText(Config.Current.LootLocation));
                    File.Copy(Config.Current.LootLocation, Config.Current.LootLocation + ".old");
                    _base = temp.Select(x => x.Value).ToList();
                    ApplyMemeTypes();
                    UpdateURLs();
                    
                    return true;
                }
                catch
                {
                    _base = new List<Meme>
                    {
                        new Meme("http://bc01.rp-online.de/polopoly_fs/63-millionen-rtl-zuschauschuldnerberatpetzwegat-familien-finanzklemme-1.503632.1315967723!httpImage/587917878.jpg_gen/derivatives/dx510/587917878.jpg", "Dummmy", Math.Round(Math.PI, 6).ToString(), MemeType.Embed)
                    };
                    return false;
                }
            }
        }

        private static void ApplyMemeTypes()
        {
            for (int i = 0; i < _base.Count; i++)
            {
                _base[i].Type = CheckURL_Type(_base[i].URL);
            }
        }

        private static void UpdateURLs()
        {
            File.WriteAllText(Config.Current.LootLocation, JsonConvert.SerializeObject(_base, Formatting.Indented));
        }

        public static (bool success, int index) AddURL(string url, string username, string id, params string[] tags)
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
                || !Config.Current.Suffixes.Any(x => url.EndsWith(x, StringComparison.InvariantCultureIgnoreCase))) 
                && !Config.Current.Prefixes.Any(x => url.StartsWith(x))
                )
                return (false, -1);
            _base.Add(new Meme(url, username, id, CheckURL_Type(url), tags.Select(x => x.ToLower()).ToArray()));
            UpdateURLs();
            return (true, _base.Count - 1);
        }

        #endregion

        #region Shortcuts

        private static bool TryLoadShortcuts()
        {
            try
            {
                _shortcuts = JsonConvert.DeserializeObject<NCS_String_Dictionary>(File.ReadAllText(Config.Current.ShortcutLocation));
                return true;
            }
            catch
            {
                _shortcuts = new NCS_String_Dictionary();
                return false;
            }
        }

        private static void UpdateShortcuts()
        {
            File.WriteAllText(Config.Current.ShortcutLocation, JsonConvert.SerializeObject(_shortcuts, Formatting.Indented));
        }

        public static bool AddShortcut(string s, int index)
        {
            if (_shortcuts.TryAdd(s, _base[index].URL))
            {
                UpdateShortcuts();
                return false;
            }
            _shortcuts.Remove(s);
            _shortcuts.Add(s, _base[index].URL);
            UpdateShortcuts();
            return true;

        }

        public static bool DeleteShortcut(string s) =>
            _shortcuts.Remove(s);

        public static (Meme m, int index) ProcessShortcut(string s)
        {
            string res = null;
            if (_shortcuts.TryGetValue(s, out res))
            {
                int index = _base.FindIndex(x => x.URL == res);
                return (_base[index], index);
            }
            else
                return (null, -1);
        }

        #endregion

        #region Tags

        public static bool AddTag(string s, int index)
        {
            if (index < 0 || index > _base.Count)
                return false;

            _base[index].Tags.Add(s.ToLower());
            UpdateURLs();
            return true;
        }

        public static bool DeleteTag(string s, int index)
        {
            if (index < 0 || index > _base.Count)
                return false;

            return _base[index].Tags.Remove(s);
        }

        public static (Meme m, int index) ProcessTag(string s)
        {
            var t = _base.Where(x => x.Tags.Contains(s.ToLower())).ToList();
            if (t.Count < 1)
                return (null, -1);

            Meme r = t.Rand().item;
            return (r, _base.IndexOf(r));
        }

        public static Dictionary<string, int> GetAllTags()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (var x in _base)
            {
                foreach (var y in x.Tags)
                {
                    if (!dic.TryAdd(y, 1))
                        dic[y]++;
                }
            }
            return new Dictionary<string, int>(dic.OrderByDescending(x => x.Value));
        }

        public static int ChangeTagGlobally(string o, string n)
        {
            var ts = _base.Where(x => x.Tags.Contains(o)).ToList();
            if (ts.Count == 0)
                return 0;

            for (int i = 0; i < ts.Count; i++)
            {
                ts[i].Tags.Remove(o);
                ts[i].Tags.Add(n);
            }

            UpdateURLs();
            return ts.Count;
        }

        #endregion
    }
}
