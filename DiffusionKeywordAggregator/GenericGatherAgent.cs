using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiffusionKeywordAggregator
{
    public abstract class GenericGatherAgent
    {
        protected string searchTerm = "";
        protected string website = "";
        protected int result = -1;
        protected long prev; // = (int)DateTime.UtcNow.Subtract(DateTime.MinValue).TotalSeconds;



        public static PublishAgent pub = new PublishAgent();


        protected GenericGatherAgent(string keyword)
        {
            searchTerm = keyword;
        }

        public async Task publish()
        {
            await pub.Run(searchTerm, website, result).ConfigureAwait(false);
        }
    }
}
