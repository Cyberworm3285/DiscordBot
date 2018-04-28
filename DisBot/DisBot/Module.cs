using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Discord.Commands;
using Discord.WebSocket;
using Discord;

using DisBot.Memes;
using DisBot.Memes.Creation;
using static DisBot.Extensions.FormatExtensions;

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
            if (Context.User.Id == Config.Current.SuperAdminID)
                return true;
            var su = Context.User as SocketGuildUser;
            if (su == null || Config.Current.CheckUserPermissions(su.Roles.Select(x => x.Name), su.Id) < (int)requiredRole)
            {
                await Context.Channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "YuNo.jpg"), $"`{Context.User}:{Context.Message}` [<{requiredRole}]");
                return false;
            }
            return true;
        }

        protected async Task ProcessMeme(IndexedMeme m)
        {
            if (m.Meme == null)
            {
                await ReplyAsync("[nix da]");
                return;
            }

            string title = $"{(m.Index == -1 ? null : "[" + m.Index + "]")}{(m.Meme.Tags.Count == 0 ? "" : " Tags[" + string.Join(",", m.Meme.Tags) + "]")}";
            switch (m.Meme.Type)
            {
                case MemeType.LinkOnly:
                    await ReplyAsync(title + " " + m.Meme.URL);
                    break;

                case MemeType.Embed:
                    var builder = new EmbedBuilder
                    {
                        Title = title,
                        Color = new Color(200, 160, 50),
                    };
                    builder.ImageUrl = m.Meme.URL;
                    await ReplyAsync("", false, builder.Build());
                    break;
            }
        }
    }

    public class MemeModule : MyModuleBase
    {


        [Command("Meme"), Alias("Gimme", "M")]
        public async Task Loot() => await ProcessMeme(Looter.Next);

        [Command("Force"), Alias("F")]
        public async Task Force(int index) => await ProcessMeme(Looter.ForceMeme(index));

        [Command("AddDirect")]
        public async Task AddLoot(params string[] tags)
        {
            if (!Context.Message.Attachments.Any())
            {
                await ReplyAsync("Ich brauch Attachements Diggha");
                return;
            }

            foreach(var x in Context.Message.Attachments)
            {
                await AddLoot(x.Url, tags);
            }
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
            if (sc.Index == -1)
            {
                await ReplyAsync($"Der Shortcut {s} is Müll du {Config.Current.RandomCurse}");
                return;
            }

            await ProcessMeme(sc);
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

        [Command("AddTags")]
        public async Task AddTags(string t, params int[] indices)
        {
            if (!await CheckUser(Roles.Memer))
                return;
            if (indices == null || indices.Length < 1)
            {
                await ReplyAsync("keine indizes angegeben");
                return;
            }
            int c = 0;

            foreach(var x in indices)
            {
                if (Looter.AddTag(t, x))
                    c++;
            }
            await ReplyAsync($"{c} tags hinzugefügt");
        }

        [Command("AddtagsUnsafe")]
        public async Task AddTags(string t, string input)
        {
            string[] parts = input.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            List<int> indices = new List<int>();
            List<string> errors = new List<string>();
            for (int i = 0; i < parts.Length; i++)
            {
                int splitIndex;
                if ((splitIndex = parts[i].IndexOf('-')) == -1)
                {
                    int res = 0;
                    if (int.TryParse(parts[i], out res))
                        indices.Add(res);
                    else
                        errors.Add(parts[i]);
                }
                else
                {
                    int a = 0, b = 0;
                    if (!int.TryParse(parts[i].Substring(0, splitIndex), out a) || !int.TryParse(parts[i].Substring(splitIndex + 1), out b))
                        errors.Add(parts[i]);
                    else
                        indices.AddRange(Enumerable.Range(a, b - a + 1));
                }
            }

            await ReplyAsync(errors.Count == 0?"keine fehler, leite weiter..":$"{errors.Count} fehler bei [{string.Join(",", errors)}], leite rest weiter");
            await AddTags(t, indices.ToArray());
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
            if (tag.Index == -1)
            {
                await ReplyAsync($"Der Tag {t} is Müll du {Config.Current.RandomCurse}");
                return;
            }

            await ProcessMeme(tag);
        }

        [Command("Tag_AND")]
        public async Task TagAnd(params string[] t)
        {
            if (t is null || t.Length == 0)
            {
                await ReplyAsync($"@{Context.User.Mention} Musst halt schon tags angeben... Spast");
                return;
            }

            var tag = Looter.ProcessTagCombination(t, TagCombiner.AND);
            if (tag.Index == -1)
            {
                await ReplyAsync($"Gibbet net");
                return;
            }

            await ProcessMeme(tag);
        }

        [Command("Tag_NAND")]
        public async Task TagNand(params string[] t)
        {
            if (t is null || t.Length == 0)
            {
                await ReplyAsync($"@{Context.User.Mention} Musst halt schon tags angeben... Spast");
                return;
            }

            var tag = Looter.ProcessTagCombination(t, TagCombiner.NAND);
            if (tag.Index == -1)
            {
                await ReplyAsync($"Gibbet net");
                return;
            }

            await ProcessMeme(tag);
        }

        [Command("Tag_OR")]
        public async Task TagOr(params string[] t)
        {
            if (t is null || t.Length == 0)
            {
                await ReplyAsync($"@{Context.User.Mention} Musst halt schon tags angeben... Spast");
                return;
            }

            var tag = Looter.ProcessTagCombination(t, TagCombiner.OR);
            if (tag.Index == -1)
            {
                await ReplyAsync($"Gibbet net");
                return;
            }

            await ProcessMeme(tag);
        }

        [Command("Tag_NOR")]
        public async Task TagNor(params string[] t)
        {
            if (t is null || t.Length == 0)
            {
                await ReplyAsync($"@{Context.User.Mention} Musst halt schon tags angeben... Spast");
                return;
            }

            var tag = Looter.ProcessTagCombination(t, TagCombiner.NOR);
            if (tag.Index == -1)
            {
                await ReplyAsync($"Gibbet net");
                return;
            }

            await ProcessMeme(tag);
        }

        [Command("Tag_Count")]
        public async Task TagCount(params string[] t)
        {
            if (t is null || t.Length == 0)
            {
                await ReplyAsync($"@{Context.User.Mention} Musst halt schon tags angeben... Spast");
                return;
            }

            var tag = Looter.ProcessTagCombination(t, TagCombiner.MAXCOUNT);
            if (tag.Index == -1)
            {
                await ReplyAsync($"Gibbet net");
                return;
            }

            await ProcessMeme(tag);
        }

        [Command("ChangeTagsGlobally")]
        public async Task ChangeTagGlobally(string o, string n)
        {
            if (!await CheckUser(Roles.Admin))
                return;
            await ReplyAsync($"Es wurden {Looter.ChangeTagGlobally(o,n)} Einträge abgeändert");
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

    public class HelpModule : MyModuleBase
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

            await ReplyAsync("Bidde", false, builder.Build());
            await ReplyAsync("Tags : \n" + string.Join("\n", Looter.GetAllTags().Select(y => $"{y.Key} [{y.Value}]")));
        }

        [Command("Find")]
        public async Task FindIndex(string URL)
        {
            int index = Looter.IndexOf(URL);
            IndexedMeme m = Looter.ForceMeme(index);
            await ReplyAsync((index == -1) ? "nichts gefunden" : $"{index}: {m}");
        }

        [Command("FindTag")]
        public async Task FindTag(string tag, bool postFull = false)
        {
            var query = Looter.Where(x => x.Tags.Contains(tag));
            if (postFull)
                foreach (var x in query)
                    await ProcessMeme(x);
            else
            {
                int i = 0;
                StringBuilder sb = new StringBuilder();
                foreach (var x in query)
                {
                    sb.Append($"{x.Index}\n[{string.Join(",",x.Meme.Tags)}] [{x.Meme.URL}]\n");
                    if (i++ > 10)
                    {
                        i = 0;
                        await ReplyAsync(sb.ToString());
                        sb.Clear();
                    }
                }
                if (i != 0)
                    await ReplyAsync(sb.ToString());
            }
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

    public class MemeCreator : ModuleBase
    {
        [Command("CreateMeme")]
        public async Task CreateMeme(int index, string upper, string lower)
        {
            IndexedMeme m = Looter.ForceMeme(index);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Memes");
            MemeCreatorManager mcm = new MemeCreatorManager(path, null);
            var res = mcm.CreateMeme(m.Meme, upper, lower);

            await Context.Channel.SendFileAsync(res.path, $"{Context.User.Username} is Schuld");

            if (Context.Guild != null)
                await Context.Channel.DeleteMessagesAsync(new[] { Context.Message.Id });

            Array.ForEach(Directory.GetFiles(path), File.Delete);
        }
    }
}
