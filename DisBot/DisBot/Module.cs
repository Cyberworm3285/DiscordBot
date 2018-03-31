﻿using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Discord.Commands;
using Discord.WebSocket;
using Discord;

namespace DisBot
{
    public class Memes : ModuleBase
    {
        [Command("Meme"), Alias("Gimme")]
        public async Task Loot()
        {
            var builder = new EmbedBuilder
            {
                Color = new Color(200, 160, 50),
            };
            builder.ImageUrl = Looter.Next;
            await ReplyAsync("", false, builder.Build());
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
        public async Task AddLoot(string url, int rarity = 1000)
        {
            if (!await CheckUser(Config.Current.AllowedRoles))
                return;
            if (!Looter.AddURL(url, rarity))
            {
                await ReplyAsync("Die Url is retarded.. glaub ich zumindest");
                return;
            }
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
                    if (Looter.AddURL(input[i + 1], r))
                        c++;
                    else
                        await ReplyAsync($"<{input[i+1]}> ist keine gültige URL");
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

        [Command("Delete")]
        public async Task Delete(string value)
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

        [Command("Dump")]
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

    public class HelpModule : ModuleBase
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("Help")]
        public async Task Help()
        {
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = "Mögliche Commands:"
            };
            foreach (var m in _service.Modules)
            {
                string moduleDesc = null;
                foreach (var c in m.Commands)
                {
                    var result = await c.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        moduleDesc += $"!{c.Aliases.First()}[{string.Join(",", c.Parameters.Select(x => $"{x.Type.Name}:{x.Name}"))}]\n";
                }
                if (!string.IsNullOrWhiteSpace(moduleDesc))
                {
                    builder.AddField(x =>
                    {
                        x.Name = m.Name;
                        x.Value = moduleDesc;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("Bidde", false, builder.Build());
        }
    }
}
