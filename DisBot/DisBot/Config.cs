using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using System.IO;
using System.Linq;

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
        public List<string> Curses { get; set; } = new List<string>{ "Hure" };
        public HashSet<string> MemerRoles { get; set; } = new HashSet<string> { "MemeGesalbter" };
        public HashSet<string> AdminRoles { get; set; } = new HashSet<string> { };
        public ulong SuperAdminID { get; set; } = 0;
        public bool AllowDuplicates { get; set; } = false;
        public bool DeleteAddRequests { get; set; } = true;
        public HashSet<string> Prefixes { get; set; } = new HashSet<string> { "https://www.youtube.com/watch?" };
        public HashSet<string> Suffixes { get; set; } = new HashSet<string> { ".png", ".jpg", ".gif", ".gifv", ".mp4" };
        public HashSet<string> ExceptionPrefixes { get; set; } = new HashSet<string> { "https://www.youtube.com/watch?" };
        public HashSet<string> ExceptionSuffixes { get; set; } = new HashSet<string> { ".gifv", ".mp4", };
        public HashSet<ulong> Blacklist { get; set; } = new HashSet<ulong>();
        public HashSet<ulong> Whitelist { get; set; } = new HashSet<ulong>();
        public bool UseTTS { get; set; } = true;

        private static Config _current;

        public static Config Current => _current ?? (_current = Load());
        [JsonIgnore]
        public string RandomCurse => Curses[rand.Next(Curses.Count)];

        private Config() { }

        public static Config Load()
        {
            try
            {
                Console.WriteLine(Directory.GetCurrentDirectory());
                _current = JsonConvert.DeserializeObject<Config>(File.ReadAllText($@"{Directory.GetCurrentDirectory()}\configBoi.cfg"));
                return _current;
            }
            catch
            {
                _current = new Config();
                _current.Write();
                return _current;
            }
        }

        public void Write() => File.WriteAllText($@"{Directory.GetCurrentDirectory()}\configBoi.cfg", JsonConvert.SerializeObject(this, Formatting.Indented));

        public int CheckUserPermissions(IEnumerable<string> roles, ulong id)
        {
            if (id == SuperAdminID)
                return 3;
            if (AdminRoles.Any(x => roles.Contains(x)))
                return 2;
            if (MemerRoles.Any(x => roles.Contains(x)))
                return 1;
            if (!Blacklist.Contains(id))
                return 0;

            return -1;
        }

        public bool IsBanned(ulong id)
        {
            return (Blacklist.Contains(id) && id != SuperAdminID);
        }

        public void AddMemer(string m) => MemerRoles.Add(m);
        public bool RemoveMemer(string m) => MemerRoles.Remove(m);

        public void AddAdmin(string a) => AdminRoles.Add(a);
        public bool RemoveAdmin(string a) => AdminRoles.Remove(a);

        public bool ToggleDuplicates() => (AllowDuplicates = !AllowDuplicates);
        public bool ToggleDeleteMessages() => (DeleteAddRequests = !DeleteAddRequests);

        public void AddPrefix(string p) => Prefixes.Add(p);
        public bool RemovePrefix(string p) => Prefixes.Remove(p);
        public void AddExceptionPrefix(string p) => ExceptionPrefixes.Add(p);
        public bool RemoveExceptionPrefix(string p) => ExceptionPrefixes.Remove(p);

        public void AddSuffix(string s) => Suffixes.Add(s);
        public bool RemoveSuffix(string s) => Suffixes.Remove(s);
        public void AddExceptionSuffix(string s) => ExceptionSuffixes.Add(s);
        public bool RemoveExceptionSuffix(string s) => ExceptionSuffixes.Remove(s);
    }
}
