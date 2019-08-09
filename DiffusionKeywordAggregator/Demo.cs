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

            r.Run().Wait();

            //Thread.Sleep(Timeout.Infinite);
        }
    }
}
