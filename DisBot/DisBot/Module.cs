using System;
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
        public async Task ProcessMeme(Meme m, int index = -1)
        {
            if (m == null)
            {
                await ReplyAsync("[nix da]");
                return;
            }

            switch (m.Type)
            {
                case MemeType.LinkOnly:
                    await ReplyAsync($"{(index==-1?null:"["+index+"]")} {m.URL}");
                    break;

                case MemeType.Embed:
                    var builder = new EmbedBuilder
                    {
                        Title = (index == -1 ? null : "[" + index + "]"),
                        Color = new Color(200, 160, 50),
                    };
                    builder.ImageUrl = m.URL;
                    await ReplyAsync("", false, builder.Build());
                    break;
            }
        }

        [Command("Meme"), Alias("Gimme", "M")]
        public async Task Loot()
        {
            var m = Looter.Next;
            await ProcessMeme(m.m, m.index);
        }

        [Command("Force")]
        public async Task Force(int index)
        {
            await ProcessMeme(Looter.ForceMeme(index), index);
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
        public async Task AddLoot(string url, params string[] tags)
        {
            if (!await CheckUser(Config.Current.AllowedRoles))
                return;
            if (Looter.Contains(url) && !Config.Current.AllowDuplicates)
            {
                await ReplyAsync("Url is schon drin ma boi");
                return;
            }
            var res = Looter.AddURL(url, Context.User.Username, Context.User.Id.ToString(), tags);
            if (!res.success)
            {
                await ReplyAsync("Die Url is retarded.. glaub ich zumindest");
                return;
            }
            await ReplyAsync($"{Context.User} hat in {Context.Channel} einen Eintrag hinzugefügt [{res.index}]");
            if (Config.Current.DeleteAddRequests)
                await Context.Channel.DeleteMessagesAsync(new[] { Context.Message.Id });
        }

        [Command("Delete")]
        public async Task Delete(string value)
        {
            int c = Looter.Delete(value);
            await ReplyAsync($"{Context.User} hat {((c != 1) ? c + " Einträge" : "einen Eintrag")} gelöscht");
        }

        [Command("Deleteindex"), Alias("KillAt")]
        public async Task DeleteIndex(int index)
        {
            await ReplyAsync(Looter.Delete(index) ? $"{Context.User} hat einen Eintrag gelöscht" : $"index {index} konnte nicht gelöscht werden");
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

        [Command("AddShortcut"),]
        public async Task AddShortcut(string s, int index)
        {
            try
            {
                if (Looter.AddShortcut(s, index))
                    await ReplyAsync($"shortcut {s} wurde überschrieben");
                else
                    await ReplyAsync($"shortcut {s} wurde erstellt");
            }
            catch (IndexOutOfRangeException)
            {
                await ReplyAsync($"index {index} ist ungültig");
            }
        }

        [Command("Shortcut"), Alias("s")]
        public async Task Shortcut(string s)
        {
            var sc = Looter.ProcessShortcut(s);
            if (!sc.exists)
            {
                await ReplyAsync($"Der Shortcut {s} is Müll du {Config.Current.RandomCurse}");
                return;
            }

            await ProcessMeme(sc.m);
        }

        [Command("AddTag")]
        public async Task AddTag(string t, int index)
        {
            if (!Looter.AddTag(t, index))
            {
                await ReplyAsync($"ne");
                return;
            }
            await ReplyAsync("okay");
        }

        [Command("Tag"), Alias("T")]
        public async Task Tag(string t)
        {
            var tag = Looter.ProcessTag(t);
            if (!tag.exists)
            {
                await ReplyAsync($"Der Tag {t} is Müll du {Config.Current.RandomCurse}");
                return;
            }

            await ProcessMeme(tag.m);
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
            builder.AddField(x => 
            {
                x.Name = "Shortcuts";
                x.Value = string.Join("\n", Looter.Shorts);
                x.IsInline = false;
            });

            await ReplyAsync("Bidde", false, builder.Build());
        }
    }
}
