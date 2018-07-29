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
    class ModuleFallback : IEnumerable<KeyValuePair<string, ReactionMeme>>
    {
        private List<KeyValuePair<string, ReactionMeme>> _keyWordsAnswers;

        public ModuleFallback()
            => _keyWordsAnswers = new List<KeyValuePair<string, ReactionMeme>>();

        public void Add(KeyValuePair<string, ReactionMeme> keywordValue)
        {
            if (_keyWordsAnswers.FindIndex(x => x.Key == keywordValue.Key) != -1)
                throw new ArgumentException($"key {keywordValue.Key} already taken");

            _keyWordsAnswers.Add(new KeyValuePair<string, ReactionMeme>(keywordValue.Key.ToUpper(), keywordValue.Value));
        }

        public async Task<bool> TryFallback(string message, CommandContext context)
        {
            //keys sind schon upper case
            KeyValuePair<string, ReactionMeme>? match = _keyWordsAnswers.Find(x => message.ToUpper().Contains(x.Key));
            if (match != null)
            {
                ReactionMeme temp = match.Value.Value;
                EmbedBuilder eb = null;
                if (!string.IsNullOrEmpty(temp.URL))
                    eb = new EmbedBuilder
                    {
                        Title = temp.Text,
                        ImageUrl = temp.URL,
                    };
                await context.Channel.SendMessageAsync($"`{context.User}>> .. {match.Value.Key} .. `{(eb == null?("\n" + temp.Text):"")}", temp.TTS, eb==null?null:eb.Build());
                return true;
            }
            return false;
        }

        public IEnumerator<KeyValuePair<string, ReactionMeme>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, ReactionMeme>>)_keyWordsAnswers).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, ReactionMeme>>)_keyWordsAnswers).GetEnumerator();
        }
    }
}
