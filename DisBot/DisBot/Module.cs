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
    public class MyModuleBase : ModuleBase
    {
        protected enum Roles
        {
            Pleb = 0,
            Memer,
            Admin,
            SuperAdmin
        }

        protected async Task<bool> CheckUser(Roles requiredRole)
        {
            var su = Context.User as SocketGuildUser;
            if (su == null || Config.Current.CheckUserPermissions(su.Roles.Select(x => x.Name), su.Id) < (int)requiredRole)
            {
                await Context.Channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "YuNo.jpg"), $"`{Context.User}:{Context.Message}` [<{requiredRole}]");
                return false;
            }
            return true;
        }
    }

    public class Memes : MyModuleBase
    {
        public async Task ProcessMeme(Meme m, int index = -1)
        {
            if (m == null)
            {
                await ReplyAsync("[nix da]");
                return;
            }

            string title = $"{(index == -1 ? null : "[" + index + "]")}{(m.Tags.Count == 0 ? "" : " Tags[" + string.Join(",", m.Tags) + "]")}";
            switch (m.Type)
            {
                case MemeType.LinkOnly:
                    await ReplyAsync(title + " " + m.URL);
                    break;

                case MemeType.Embed:
                    var builder = new EmbedBuilder
                    {
                        Title = title,
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

        [Command("Force"), Alias("F")]
        public async Task Force(int index)
        {
            await ProcessMeme(Looter.ForceMeme(index), index);
        }

        [Command("Add"), Alias("A")]
        public async Task AddLoot(string url, params string[] tags)
        {
            if (!await CheckUser(Roles.Memer))
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
            await ReplyAsync("das willst du nicht");
            return;
            //if (!await CheckUser(Roles.SuperAdmin))
            //    return;
            //
            //Looter.Flush();
            //
            //await ReplyAsync("Alles gelöscht");
        }

        [Command("Dump")]
        public async Task GetMemes()
        {
            string path = $@"{Directory.GetCurrentDirectory()}\Dump_{Guid.NewGuid()}.html";
            File.WriteAllText(path, Looter.GetHTMLFormattedOverview());
            await Context.Channel.SendFileAsync(path, $":point_down: {Config.Current.RandomCurse} <3");
            File.Delete(path);
        }

        [Command("AddShortcut"), Alias("AS")]
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
            if (sc.index == -1)
            {
                await ReplyAsync($"Der Shortcut {s} is Müll du {Config.Current.RandomCurse}");
                return;
            }

            await ProcessMeme(sc.m);
        }

        [Command("AddTag"), Alias("AT")]
        public async Task AddTag(string t, int index)
        {
            if (!Looter.AddTag(t, index))
            {
                await ReplyAsync($"ne");
                return;
            }
            await ReplyAsync("okay");
        }

        [Command("DeleteTag"), Alias("DT")]
        public async Task DeleteTag(string t, int index)
        {
            if(!Looter.DeleteTag(t, index))
            {
                await ReplyAsync("ne");
                return;
            }
            await ReplyAsync("okay");
        }

        [Command("Tag"), Alias("T")]
        public async Task Tag(string t)
        {
            var tag = Looter.ProcessTag(t);
            if (tag.index == -1)
            {
                await ReplyAsync($"Der Tag {t} is Müll du {Config.Current.RandomCurse}");
                return;
            }

            await ProcessMeme(tag.m);
        }
    }

    [Group("Config")]
    public class Configuration : MyModuleBase
    {

        private async Task Okay() => await ReplyAsync("Okay");

        [Command("RW")]
        public async Task ReWriteConfig()
        {
            if (!await CheckUser(Roles.Admin))
                return;
            Config.Current.Write();
            await ReplyAsync("Config rewritten");
        }

        [Command("RL")]
        public async Task LoadConfig()
        {
            if (!await CheckUser(Roles.Admin))
                return;
            Config.Load();
            await ReplyAsync("Config loaded");
        }

        [Command("AddMemer")]
        public async Task AddMemer(string m)
        {
            if (!await CheckUser(Roles.Admin))
                return;
            Config.Current.AddMemer(m);
            await ReplyAsync("okay");
        }

        [Command("RemoveMemer")]
        public async Task RemoveMemer(string m)
        {
            if (!await CheckUser(Roles.Admin))
                return;
            await ReplyAsync(Config.Current.RemoveMemer(m).OkayNe());
        }

        [Command("AddAdmin")]
        public async Task AddAdmin(string a)
        {
            if (!await CheckUser(Roles.SuperAdmin))
                return;
            Config.Current.AddAdmin(a);
            await Okay();
        }

        [Command("RemoveAdmin")]
        public async Task RemoveAdmin(string a)
        {
            if (!await CheckUser(Roles.SuperAdmin))
                return;
            await ReplyAsync(Config.Current.RemoveAdmin(a).OkayNe());
        }

        [Command("ToggleDuplicates")]
        public async Task ToggleDuplicates()
        {
            if (!await CheckUser(Roles.Admin))
                return;
            await ReplyAsync($"Duplikate sind {Config.Current.ToggleDuplicates().EinAus()}");
        }

        [Command("ToggleDelete")]
        public async Task ToggleDeleteMessages()
        {
            if (!await CheckUser(Roles.Admin))
                return;
            await ReplyAsync($"Löschen ist {Config.Current.ToggleDeleteMessages().EinAus()}");
        }

        [Command("AddPrefix")]
        public async Task AddPrefix(string p)
        {
            if (!await CheckUser(Roles.SuperAdmin))
                return;
            Config.Current.AddPrefix(p);
            await Okay();
        }

        [Command("RemovePrefix")]
        public async Task RemovePrefix(string p)
        {
            if (!await CheckUser(Roles.SuperAdmin))
                return;
            await ReplyAsync(Config.Current.RemovePrefix(p).OkayNe());
        }

        [Command("AddExceptionPrefix")]
        public async Task AddExceptionPrefix(string p)
        {
            if (!await CheckUser(Roles.SuperAdmin))
                return;
            Config.Current.AddExceptionPrefix(p);
            await Okay();
        }

        [Command("RemoveExceptionPrefix")]
        public async Task RemoveExceptionPrefix(string p)
        {
            if (!await CheckUser(Roles.SuperAdmin))
                return;
            await ReplyAsync(Config.Current.RemoveExceptionPrefix(p).OkayNe());
        }

        [Command("AddSuffix")]
        public async Task AddSuffix(string s)
        {
            if (!await CheckUser(Roles.SuperAdmin))
                return;
            Config.Current.AddSuffix(s);
            await Okay();
        }

        [Command("RemoveSuffix")]
        public async Task RemoveSuffix(string s)
        {
            if (!await CheckUser(Roles.SuperAdmin))
                return;
            await ReplyAsync(Config.Current.RemoveSuffix(s).OkayNe());
        }

        [Command("AddExceptionSuffix")]
        public async Task AddExceptionSuffix(string s)
        {
            if (!await CheckUser(Roles.SuperAdmin))
                return;
            Config.Current.AddExceptionSuffix(s);
            await Okay();
        }

        [Command("RemoveExceptionPrefix")]
        public async Task RemoveExceptionSuffix(string s)
        {
            if (!await CheckUser(Roles.SuperAdmin))
                return;
            await ReplyAsync(Config.Current.RemoveExceptionSuffix(s).OkayNe());
        }
    }

    public class HelpModule : ModuleBase
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("Help"), Alias("?")]
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
                        moduleDesc += $"![{(string.Join(",", c.Aliases))}][{string.Join(",", c.Parameters.Select(x => $"{x.Type.Name}:{x.Name}"))}]\n";
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
            builder.AddField(x =>
            {
                x.Name = "Tags";
                x.Value = string.Join("\n", Looter.GetAllTags().Select(y => $"{y.Key} [{y.Value}]"));
                x.IsInline = false;
            });

            await ReplyAsync("Bidde", false, builder.Build());
        }

        [Command("Find")]
        public async Task FindIndex(string URL)
        {
            int index = Looter.IndexOf(URL);
            await ReplyAsync((index == -1) ? "nichts gefunden" : index.ToString());
        }

        [Command("Stats")]
        public async Task Stats()
        {
            EmbedBuilder sb = new EmbedBuilder()
            {
                Title = $"Alle stats ({Looter.Count} Meme(s))",
                Color = new Color(114, 137, 218)
            };

            var stats = Looter.GetStats();
            foreach (var x in stats.OrderByDescending(s => s.Value))
            {
                sb.AddField(y =>
                {
                    y.Name = x.Key;
                    y.Value = x.Value;
                });
            }

            await ReplyAsync("Bidde", false, sb.Build());
        }
    }
}
