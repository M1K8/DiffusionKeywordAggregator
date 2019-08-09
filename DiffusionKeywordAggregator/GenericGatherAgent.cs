using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiffusionKeywordAggregator
{
    abstract class GenericGatherAgent
    {
        protected PublishAgent sendBoi = new PublishAgent();
        protected static string searchTerm = "";
        protected string website = "";
        protected int result = -1;
        protected long prev; // = (int)DateTime.UtcNow.Subtract(DateTime.MinValue).TotalSeconds;

        private static PublishAgent pub = new PublishAgent();

        protected GenericGatherAgent(string keyword)
        {
            searchTerm = keyword;
        }

        public async Task publish()
        {
            await pub.Run(searchTerm, website, result);
        }

        //construct JSON object for each class to push to PublishAgent.cs
        ///protected abstract string OnHitCallback();
    }
}
