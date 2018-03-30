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
    }
}
