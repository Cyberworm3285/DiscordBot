using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using System.IO;

namespace DisBot
{
    [Serializable]
    class Config
    {
        [JsonIgnore]
        private static Random rand = new Random();
        public string Token { get; set; } = null;
        public string LootLocation { get; set; } = $@"{Directory.GetCurrentDirectory()}\loot.lt";
        public string ShortcutLocation { get; set; } = $@"{Directory.GetCurrentDirectory()}\shortcuts.sc";
        public string[] Curses { get; set; } = { "Hure" };
        public string[] AllowedRoles { get; set; } = { "MemeGesalbter" };
        public string[] FlushRoles { get; set; } = { };
        public bool AllowDuplicates { get; set; } = false;
        public bool DeleteAddRequests { get; set; } = true;
        public string[] Prefixes { get; set; } = new[] { "https://www.youtube.com/watch?" };
        public string[] Suffixes { get; set; } = new[] { ".png", ".jpg", "gif", ".gifv", ".mp4" };

        private static Config _current;

        public static Config Current => _current ?? (_current = Load());
        [JsonIgnore]
        public string RandomCurse => Curses[rand.Next(Curses.Length)];

        private Config() { }

        public static Config Load()
        {
            try
            {
                Console.WriteLine(Directory.GetCurrentDirectory());
                _current = JsonConvert.DeserializeObject<Config>(File.ReadAllText($@"{Directory.GetCurrentDirectory()}\config.cfg"));
                return _current;
            }
            catch
            {
                _current = new Config();
                _current.Write();
                return _current;
            }
        }

        public void Write() => File.WriteAllText($@"{Directory.GetCurrentDirectory()}\config.cfg", JsonConvert.SerializeObject(this, Formatting.Indented));
    }
}
