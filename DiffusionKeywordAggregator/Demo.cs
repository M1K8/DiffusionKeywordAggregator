using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DiffusionKeywordAggregator
{
    class Demo
    {
        static void Main(string[] args)
        {
            var r = new RedditAgent("yeet");

            var t = new TwitterAgent("yeet");


            r.Run();

            //t.Run();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
