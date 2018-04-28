using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

using DisBot.Memes;

namespace DisBot.Extensions
{
    public static class LootingExtensions
    {
        private static Random rand = new Random();

        public static T Rand<T>(this IEnumerable<T> e)
        {
            var t = e.ToArray();
            return t[rand.Next(t.Length)];
        }

        public static IndexedMeme Rand(this List<Meme> l)
        {
            int index = rand.Next(l.Count);
            return new IndexedMeme(l[index], index);
        }

        public static IEnumerable<IndexedMeme> IndexedWhere(this List<Meme> list, Predicate<Meme> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    yield return new IndexedMeme(list[i], i);
            }
        }
    }
}
