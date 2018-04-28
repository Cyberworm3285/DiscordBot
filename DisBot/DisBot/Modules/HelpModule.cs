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
            if (!await RequireUser(Roles.SuperAdmin))
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

        [Command("Ban")]
        public async Task Ban(string user)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;

            Context.Message.MentionedUserIds.ToList().ForEach(x => Config.Current.Blacklist.Add(x));
        }

        [Command("UnBan")]
        public async Task UnBan(string user)
        {
            if (!await RequireUser(Roles.SuperAdmin))
                return;

            Context.Message.MentionedUserIds.ToList().ForEach(x => Config.Current.Blacklist.Remove(x));
        }
    }
}
