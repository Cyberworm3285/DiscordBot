using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;

using Discord.Commands;

using static DisBot.Extensions.FormatExtensions;

namespace DisBot.Modules
{
    [Group("Config")]
    public class ConfigModule : MyModuleBase
    {
        private async Task Okay() => await ReplyAsync("Okay");

        [Command("RW")]
        public async Task ReWriteConfig()
        {
            if (!await RequireUser(Roles.Admin))
                return;
            Config.Current.Write();
            await ReplyAsync("Config rewritten");
        }

        [Command("RL")]
        public async Task LoadConfig()
        {
            if (!await RequireUser(Roles.Admin))
                return;
            Config.Load();
            await ReplyAsync("Config loaded");
        }

        [Command("AddMemer")]
        public async Task AddMemer(string m)
        {
            if (!await RequireUser(Roles.Admin))
                return;
            Config.Current.AddMemer(m);
            await ReplyAsync("okay");
        }

        [Command("RemoveMemer")]
        public async Task RemoveMemer(string m)
        {
            if (!await RequireUser(Roles.Admin))
                return;
            await ReplyAsync(Config.Current.RemoveMemer(m).OkayNe());
        }

        [Command("AddAdmin")]
        public async Task AddAdmin(string a)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;
            Config.Current.AddAdmin(a);
            await Okay();
        }

        [Command("RemoveAdmin")]
        public async Task RemoveAdmin(string a)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;
            await ReplyAsync(Config.Current.RemoveAdmin(a).OkayNe());
        }

        [Command("ToggleDuplicates")]
        public async Task ToggleDuplicates()
        {
            if (!await RequireUser(Roles.Admin))
                return;
            await ReplyAsync($"Duplikate sind {Config.Current.ToggleDuplicates().EinAus()}");
        }

        [Command("ToggleDelete")]
        public async Task ToggleDeleteMessages()
        {
            if (!await RequireUser(Roles.Admin))
                return;
            await ReplyAsync($"Löschen ist {Config.Current.ToggleDeleteMessages().EinAus()}");
        }

        [Command("AddPrefix")]
        public async Task AddPrefix(string p)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;
            Config.Current.AddPrefix(p);
            await Okay();
        }

        [Command("RemovePrefix")]
        public async Task RemovePrefix(string p)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;
            await ReplyAsync(Config.Current.RemovePrefix(p).OkayNe());
        }

        [Command("AddExceptionPrefix")]
        public async Task AddExceptionPrefix(string p)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;
            Config.Current.AddExceptionPrefix(p);
            await Okay();
        }

        [Command("RemoveExceptionPrefix")]
        public async Task RemoveExceptionPrefix(string p)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;
            await ReplyAsync(Config.Current.RemoveExceptionPrefix(p).OkayNe());
        }

        [Command("AddSuffix")]
        public async Task AddSuffix(string s)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;
            Config.Current.AddSuffix(s);
            await Okay();
        }

        [Command("RemoveSuffix")]
        public async Task RemoveSuffix(string s)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;
            await ReplyAsync(Config.Current.RemoveSuffix(s).OkayNe());
        }

        [Command("AddExceptionSuffix")]
        public async Task AddExceptionSuffix(string s)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;
            Config.Current.AddExceptionSuffix(s);
            await Okay();
        }

        [Command("RemoveExceptionPrefix")]
        public async Task RemoveExceptionSuffix(string s)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;
            await ReplyAsync(Config.Current.RemoveExceptionSuffix(s).OkayNe());
        }
    }
}
