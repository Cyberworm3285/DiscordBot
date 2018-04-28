using System;
using System.Collections.Generic;
using System.Text;

namespace DisBot.Memes
{
    public readonly struct IndexedMeme
    {
        public readonly Meme Meme;
        public readonly int Index;

        public IndexedMeme(Meme m, int i)
        {
            Meme = m;
            Index = i;
        }
    }
}
