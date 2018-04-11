using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

namespace DisBot
{
    public static class Extensions
    {
        private static Random rand = new Random();

        public static T Rand<T>(this IEnumerable<T> e)
        {
            var t = e.ToArray();
            return t[rand.Next(t.Length)];
        }

        public static (T item, int index) Rand<T>(this List<T> l)
        {
            int index = rand.Next(l.Count);
            return (l[index], index);
        }

        public static string OkayNe(this bool b) => (b) ? "Okay" : "Ne";
        public static string EinAus(this bool b) => (b) ? "An" : "Aus";
    }
}
