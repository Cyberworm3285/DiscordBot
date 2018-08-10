using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using DisBot.Memes.ReactionMemes;
using Discord;
using Discord.WebSocket;

using static DisBot.Extensions.LootingExtensions;

namespace DisBot.Modules.Fallback
{
    class ModuleFallback : IEnumerable<(IMessageMatcher, IReactionProvider)>
    {
        private List<(IMessageMatcher Matcher, IReactionProvider Provider)> _reactionMemes;

        public ModuleFallback()
            => _reactionMemes = new List<(IMessageMatcher, IReactionProvider)>();

        public ModuleFallback(ICollection<(IMessageMatcher, IReactionProvider)> col)
            => _reactionMemes = col.ToList();

        public void Add((IMessageMatcher, IReactionProvider) meme)
            =>_reactionMemes.Add(meme);

        public async Task<bool> TryFallback(SocketUserMessage message, CommandContext context)
        {
            //keys sind schon upper case
            var list = _reactionMemes
                .Where(x => x.Matcher.Match(message))
                .GroupBy(x => x.Matcher.Priority)
                .OrderByDescending(x => x.Key)
                .First()
                .ToList();

            if (list.Count > 1 && list[0].Matcher.Priority > 0)
                await context.Channel.SendMessageAsync($"Bot-Admin, get your priorities straight du Lachsnacken (c:{list.Count},p:{list[0].Matcher.Priority})");

            ReactionMeme temp = list.Rand().Provider.GetReaction(message);
            EmbedBuilder eb = null;
            if (!string.IsNullOrEmpty(temp.URL))
            {
                eb = new EmbedBuilder
                {
                    Title = temp.Text,
                    ImageUrl = temp.URL,
                    Color = new Color(255, 100, 0),
                };
                await context.Channel.SendMessageAsync("", false, eb.Build());
            }
            await context.Channel.SendMessageAsync(temp.Text, temp.TTS && Config.Current.UseTTS);
            return true;
        }

        public IEnumerator<(IMessageMatcher, IReactionProvider)> GetEnumerator()
        {
            return ((IEnumerable<(IMessageMatcher, IReactionProvider)>)_reactionMemes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<(IMessageMatcher, IReactionProvider)>)_reactionMemes).GetEnumerator();
        }
    }
}
