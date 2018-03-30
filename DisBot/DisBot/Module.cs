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

        [Command("Add")]
        public async Task AddLoot(int rarity, string url)
        {
            var su = Context.User as SocketGuildUser;
            if (!su.Roles.Any(x => Config.Current.AllowedRoles.Contains(x.Name)))
            {
                await Context.Channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(),"YuNo.jpg"),$"`{Context.User}:{Context.Message}`");
                return;
            }
            Looter.AddURL(url, rarity);
            Config.Current.Write();
            await ReplyAsync($"{Context.User} hat in {Context.Channel} einen Eintrag hinzugefügt");
            await Context.Channel.DeleteMessagesAsync(new[] { Context.Message.Id });
        }

        [Command("DeLast")]
        public async Task DeleteLast()
        {
            int c = Looter.Delete(Looter.Last);
            await ReplyAsync($"{Context.User} hat {((c != 1)?c + "Einträge":"einen Eintrag")} gelöscht");
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
        [Command("RCFG")]
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
