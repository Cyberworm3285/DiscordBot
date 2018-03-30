using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Discord.Commands;
using Discord.WebSocket;


namespace DisBot
{
    public class Looting : ModuleBase
    {
        [Command("Meme"), Alias("Gimme")]
        public async Task Loot()
        {
            await ReplyAsync(Looter.Next);
        }

        private async Task<bool> CheckUser(string[] roles)
        {
            var su = Context.User as SocketGuildUser;
            if (!su.Roles.Any(x => roles.Contains(x.Name)))
            {
                await Context.Channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "YuNo.jpg"), $"`{Context.User}:{Context.Message}`");
                return false;
            }
            return true;
        }

        [Command("Add")]
        public async Task AddLoot(int rarity, string url)
        {
            if (!await CheckUser(Config.Current.AllowedRoles))
                return;

            Looter.AddURL(url, rarity);
            await ReplyAsync($"{Context.User} hat in {Context.Channel} einen Eintrag hinzugefügt");
            await Context.Channel.DeleteMessagesAsync(new[] { Context.Message.Id });
        }

        [Command("AddRange")]
        public async Task AddRange(params string[] input)
        {
            if (!await CheckUser(Config.Current.AllowedRoles))
                return;

            int c = 0;
            for (int i = 0; i < input.Length; i+=2)
            {
                int r = 0;
                if (int.TryParse(input[i], out r))
                {
                    Looter.AddURL(input[i + 1], r);
                    c++;
                }
                else
                    await ReplyAsync($"Keine Zahl ({input[i]}), Element {i/2+1} wird übersprungen");
            }
            await ReplyAsync($"{Context.User} hat ({c}/{input.Length/2}) Element/e erfolgreich hinzugefügt");
        }

        [Command("DeLast")]
        public async Task DeleteLast()
        {
            int c = Looter.Delete(Looter.Last);
            await ReplyAsync($"{Context.User} hat {((c != 1)?c + " Einträge":"einen Eintrag")} gelöscht");
        }

        public async Task Delete(string value, int count)
        {
            int c = Looter.Delete(value);
            await ReplyAsync($"{Context.User} hat {((c != 1) ? c + " Einträge" : "einen Eintrag")} gelöscht");
        }

        [Command("Flush")]
        public async Task Flush()
        {
            if (!await CheckUser(Config.Current.FlushRoles))
                return;

            Looter.Flush();

            await ReplyAsync("Alles gelöscht");
        }

        [Command("HtmlMemeDump")]
        public async Task GetMemes()
        {
            string path = $@"{Directory.GetCurrentDirectory()}\Dump_{Guid.NewGuid()}.html";
            File.WriteAllText(path, Looter.GetHTMLFormattedOverview());
            await Context.Channel.SendFileAsync(path, $":point_down: {Config.Current.RandomCurse} <3");
            File.Delete(path);
        }
    }

    public class Configuration : ModuleBase
    {
        [Command("RWCFG")]
        public async Task ReWriteConfig()
        {
            Config.Current.Write();
            await ReplyAsync("Config rewritten");
        }

        [Command("RLCFG")]
        public async Task LoadConfig()
        {
            Config.Load();
            await ReplyAsync("Config loaded");
        }
    }
}
