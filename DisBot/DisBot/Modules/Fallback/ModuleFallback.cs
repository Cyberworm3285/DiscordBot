using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using DisBot.Memes;
using Discord;

namespace DisBot.Modules.Fallback
{
    class ModuleFallback : IEnumerable<ReactionMeme>
    {
        private List<ReactionMeme> _reactionMemes;

        public ModuleFallback()
            => _reactionMemes = new List<ReactionMeme>();

        public void Add(ReactionMeme meme)
            =>_reactionMemes.Add(meme);

        public async Task<bool> TryFallback(string message, CommandContext context)
        {
            //keys sind schon upper case
            var list = _reactionMemes
                .Where(x => x.Matcher(message))
                .ToList();

            if (list.Count > 1)
                await context.Channel.SendMessageAsync("[mehrfachbelegung, Bot-Admin ist ein Idiot]");

            ReactionMeme temp = list[0];
            EmbedBuilder eb = null;
            if (!string.IsNullOrEmpty(temp.URL))
            {
                eb = new EmbedBuilder
                {
                    Title = temp.Transformer(message, context),
                    ImageUrl = temp.URL,
                };
                await context.Channel.SendMessageAsync("", false, eb.Build());
            }
            await context.Channel.SendMessageAsync(temp.Transformer(message, context), temp.TTS);
            return true;
        }

        public IEnumerator<ReactionMeme> GetEnumerator()
        {
            return ((IEnumerable<ReactionMeme>)_reactionMemes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ReactionMeme>)_reactionMemes).GetEnumerator();
        }
    }
}
