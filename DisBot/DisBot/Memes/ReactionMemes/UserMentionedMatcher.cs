using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

using Discord.WebSocket;

namespace DisBot.Memes.ReactionMemes
{
    class UserMentionedMatcher : IMessageMatcher
    {
        private readonly ulong _id;
        public int Priority => 0;

        public UserMentionedMatcher(ulong id)
            => _id = id;

        public bool Match(SocketUserMessage message)
            => message.MentionedUsers.Select(x => x.Id).Contains(_id);
    }
}
