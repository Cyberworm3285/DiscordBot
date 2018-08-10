using System;
using System.Collections.Generic;
using System.Text;

namespace DisBot.Memes.ReactionMemes
{
    static class ReactionLibrary
    {
        public static (IMessageMatcher, IReactionProvider)[] GetReactions()
            => new (IMessageMatcher, IReactionProvider)[]
            {
                (new KeyWordMatcher("hans"), new PlainReactionProvider(":fire: Get Ze Flammenwerfer :fire:")),
                (new KeyWordMatcher("kommste ran"), new PlainReactionProvider("","https://i.redd.it/oz4ds1ecg5r01.gif")),
                (new KeyWordMatcher("marco"), new PlainReactionProvider("pooloo", null, true)),
                (new KeyWordMatcher("cancer"), new PlainReactionProvider("Cancer", null, true)),
                (new RandomMatcher(0.01), new RandomMessageProvider(
                    new PlainReactionProvider("Gay"),
                    new PlainReactionProvider("Hure")
                )),
                (new UserMentionedMatcher(Config.Current.SelfID), new PlainReactionProvider("Wat willst du denn jetz?")),
            };
    }
}
