using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DiffusionKeywordAggregator
{
    public static class Demo
    {
        public static void Main(string[] args)
        {
            var r = new RedditAgent("yeet");
            var t = new TwitterAgent("yeet");

            r.Run();
            t.Run().Wait();
        }
    }
}
    