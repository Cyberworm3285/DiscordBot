using System;
using System.Collections.Generic;
using System.Text;

namespace DisBot.Extensions
{
    public static class FormatExtensions
    {
        public static string OkayNe(this bool b) => (b) ? "Okay" : "Ne";
        public static string EinAus(this bool b) => (b) ? "An" : "Aus";
    }
}
