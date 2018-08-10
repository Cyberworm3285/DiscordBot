using System;
using System.Collections.Generic;
using System.Text;

using Discord.WebSocket;

namespace DisBot.Memes.ReactionMemes
{
    class RandomMatcher : IMessageMatcher
    {
        private readonly double _prob;
        private Random _rand;

        public int Priority => 0;

        public RandomMatcher(double prob)
            => (_prob, _rand) = (prob, new Random());

        public bool Match(SocketUserMessage message)
            => _rand.NextDouble() <= _prob;
    }
}
