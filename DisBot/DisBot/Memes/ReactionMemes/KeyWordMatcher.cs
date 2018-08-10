using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

using Discord.WebSocket;

namespace DisBot.Memes.ReactionMemes
{
    class KeyWordMatcher : IMessageMatcher
    {
        private readonly string[] _keyWords;

        public int Priority => 1;

        public KeyWordMatcher(params string[] keywords)
            => _keyWords = keywords;

        public bool Match(SocketUserMessage message)
            => _keyWords.Any(x => x.ToUpper().Contains(message.Content.ToUpper()));
    }
}
