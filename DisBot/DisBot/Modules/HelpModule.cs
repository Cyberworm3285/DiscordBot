using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;
using System.Linq;

using Discord;
using Discord.Commands;

using DisBot.Memes;

namespace DisBot.Modules
{
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

            StringBuilder sb = new StringBuilder();
            var tags = Looter.GetAllTags()
                .Select(y => $"{y.Key} [{y.Value}]")
                .ToList();
            int length = 0;

            string NEWLINE = Environment.NewLine;
            string TAG_HEADER = "Tags : " + NEWLINE;
            int ADDITIONAL_LENGTH = (TAG_HEADER + NEWLINE).Length;

            for (int i = 0; i < tags.Count; i++)
            {
                if (length + tags[i].Length + ADDITIONAL_LENGTH >= 2000)
                {
                    await ReplyAsync(TAG_HEADER + sb.ToString());
                    length = 0;
                    sb.Clear();
                }
                sb.Append(tags[i] + NEWLINE);
                length += tags[i].Length + 2;
            }
            if (length > 0)
                await ReplyAsync(TAG_HEADER + sb.ToString());
        }

        [Command("Find")]
        public async Task FindIndex(string URL)
        {
            int index = Looter.IndexOf(URL);
            IndexedMeme m = Looter.ForceMeme(index);
            await ReplyAsync((index == -1) ? "nichts gefunden" : $"{index}: {m.Meme}");
        }

        [Command("FindTag")]
        public async Task FindTag(string tag, bool postFull = false)
        {
            if (!await RequireUser(Roles.Admin))
                return;
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
                    sb.Append($"{x.Index}[{string.Join(",", x.Meme.Tags)}]\n[{x.Meme.URL}]\n");
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

        //[Command("FindDeadWeight")]
        //public async Task FindCorrupted(bool kill = false)
        //{
        //    return;
        //    new Task(async () =>
        //    {
        //        if (!await RequireUser(Roles.SuperAdmin))
        //            return;
        //
        //        var r = Looter.FindCorupptedUrls().ToList();
        //        if (!r.Any())
        //        {
        //            await ReplyAsync("alles paletti");
        //            return;
        //        }
        //        if (kill)
        //        {
        //            int kills = 0;
        //            for (int i = r.Count - 1; i > 0; i--)
        //            {
        //                Looter.Delete(r[i]);
        //                await ReplyAsync($"Pew [{r[i]}]");
        //                kills++;
        //            }
        //            await ReplyAsync($"{kills} kills");
        //            return;
        //        }
        //        await ReplyAsync("betroffene Indizes:");
        //        foreach(var x in r)
        //            await ReplyAsync(x.ToString());
        //    }).Start();
        //}

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

        [Command("GetUrl")]
        public async Task GetURL(int index)
        {
            await ReplyAsync($"[{Looter.ForceMeme(index).Meme.URL}]");
        }

    }
}
