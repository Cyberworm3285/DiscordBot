using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisBot.Memes.ReactionMemes
{
    interface IMessageMatcher
    {
        int Priority { get; }

        bool Match(SocketUserMessage message);
    }
}
