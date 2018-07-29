using System;
using System.Collections.Generic;
using System.Text;

namespace DisBot.Memes
{
    struct ReactionMeme
    {
        public readonly string Text;
        public readonly bool TTS;
        public readonly string URL;

        public ReactionMeme(string t, string url, bool tts)
            => (Text, URL, TTS) = (t, url, tts);
    }
}
