using System;
using System.Collections.Generic;
using System.Text;

using Discord.WebSocket;

using static DisBot.Extensions.LootingExtensions;

namespace DisBot.Memes.ReactionMemes
{
    class RandomMessageProvider : IReactionProvider
    {
        private IReactionProvider[] _reactionProviders;
        private Random _rand;

        public RandomMessageProvider(params IReactionProvider[] reactions)
            => (_reactionProviders, _rand) = (reactions, new Random());

        public ReactionMeme GetReaction(SocketUserMessage message)
            => _reactionProviders.Rand().GetReaction(message);
    }
}
