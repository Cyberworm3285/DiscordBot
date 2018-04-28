using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;
using System.Linq;
using System.IO;

using Discord.Commands;

using DisBot.Memes;

namespace DisBot.Modules
{
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

            foreach (var x in Context.Message.Attachments)
            {
                await AddLoot(x.Url, tags);
            }
        }

        [Command("Add"), Alias("A")]
        public async Task AddLoot(string url, params string[] tags)
        {
            if (!await RequireUser(Roles.Memer))
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
            if (!await RequireUser(Roles.Memer))
                return;
            if (indices == null || indices.Length < 1)
            {
                await ReplyAsync("keine indizes angegeben");
                return;
            }
            int c = 0;

            foreach (var x in indices)
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

            await ReplyAsync(errors.Count == 0 ? "keine fehler, leite weiter.." : $"{errors.Count} fehler bei [{string.Join(",", errors)}], leite rest weiter");
            await AddTags(t, indices.ToArray());
        }

        [Command("DeleteTag"), Alias("DT")]
        public async Task DeleteTag(string t, int index)
        {
            if (!Looter.DeleteTag(t, index))
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
            if (!await RequireUser(Roles.Admin))
                return;
            await ReplyAsync($"Es wurden {Looter.ChangeTagGlobally(o, n)} Einträge abgeändert");
        }
    }
}
