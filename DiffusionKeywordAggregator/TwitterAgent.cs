using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Parameters;

namespace DiffusionKeywordAggregator
{
    class TwitterAgent : GenericGatherAgent
    {
        static readonly HttpClient client = new HttpClient();


        //from https://github.com/client9/snowflake2time/blob/master/python/snowflake.py
        private long utc2snow(long u)
        {
            return ((long) (Math.Round((double)u * 1000) - 1288834974657)) << 22;
        }

        public TwitterAgent(string keyword) : base(keyword)
        {
            

            website = "twitter";

            result = 0;

            prev = ((long)(DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds) - 20);
        }

        public async Task Run()
        {
            while (true)
            {
                result = 0;
                var res = Search.SearchTweets(new SearchTweetsParameters(searchTerm)
                {
                    MaximumNumberOfResults = 2000,
                    SinceId = utc2snow(prev)

                });
                foreach (var e in res)
                    result += 1;

                await publish();
                Console.WriteLine(utc2snow(prev));
                prev = ((long)(DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds) - 20);
                Thread.Sleep(TimeSpan.FromMilliseconds(20000));
            }
        }
    }
}
