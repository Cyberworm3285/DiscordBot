using System;
using System.Collections.Generic;
using System.Text;

using Discord.WebSocket;

namespace DisBot.Memes.ReactionMemes
{
    class PlainReactionProvider : IReactionProvider
    {
        private readonly ReactionMeme _reactionMeme;

        public PlainReactionProvider(string text, string url = null, bool tts = false)
            => _reactionMeme = new ReactionMeme(text, url, tts);

        public ReactionMeme GetReaction(SocketUserMessage message)
            => _reactionMeme;
    }
}
