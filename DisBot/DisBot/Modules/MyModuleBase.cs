using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;
using System.Linq;
using System.IO;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using DisBot.Memes;

namespace DisBot.Modules
{
    public abstract class MyModuleBase : ModuleBase
    {
        protected enum Roles
        {
            Banned = -1,
            Pleb,
            Memer,
            Admin,
            SuperAdmin
        }

        protected async Task<bool> RequireUser(Roles requiredRole)
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
}
