using System;
using System.Collections.Generic;
using System.Text;

using Discord.Commands;

namespace DisBot.Memes
{
    class ReactionMeme
    {
        public readonly bool TTS;
        public readonly string URL;
        public readonly Func<string, bool> Matcher;
        public readonly Func<string, CommandContext, string> Transformer;

        public ReactionMeme(string url, bool tts, Func<string, bool> matcher, Func<string, CommandContext, string> transformer)
            => (URL, TTS, Matcher, Transformer) = (url, tts, matcher, transformer);
    }
}
