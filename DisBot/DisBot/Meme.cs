using System;
using System.Collections.Generic;
using System.Text;

namespace DisBot
{
    class Meme
    {
        public string URL { get; set; }
        public string Username { get; set; }
        public string ID { get; set; }

        public Meme(string url, string usermane, string id)
        {
            URL = url;
            Username = usermane;
            ID = id;
        }

        private Meme() { }
    }
}
