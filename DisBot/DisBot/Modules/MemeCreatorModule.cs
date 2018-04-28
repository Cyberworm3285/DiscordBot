using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;
using System.IO;

using Discord.Commands;

using DisBot.Memes;
using DisBot.Memes.Creation;

namespace DisBot.Modules
{
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
