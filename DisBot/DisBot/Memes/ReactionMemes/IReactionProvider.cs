using System;
using System.Collections.Generic;
using System.Text;

using Discord.WebSocket;

namespace DisBot.Memes.ReactionMemes
{
    interface IReactionProvider
    {
        ReactionMeme GetReaction(SocketUserMessage message);
    }
}
