using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiffusionKeywordAggregator
{
    class TwitterAgent : GenericGatherAgent
    {
        public TwitterAgent(string keyword) : base(keyword)
        {
            website = "twitter";

            result = 5;
        }

        public async Task Run()
        {
            while (true)
            {
                await publish();
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
