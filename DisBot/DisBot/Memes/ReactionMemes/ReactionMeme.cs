using System;
using System.Collections.Generic;
using System.Text;

using Discord.Commands;

namespace DisBot.Memes.ReactionMemes
{
    class ReactionMeme
    {
        public readonly bool TTS;
        public readonly string URL;
        public readonly string Text;

        public ReactionMeme(string text, string url = null, bool tts = false)
            => (Text, URL, TTS) = (text, url, tts);
    }
}
