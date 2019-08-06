using System;
using System.Collections.Generic;
using System.Text;

namespace DiffusionKeywordAggregator
{
    abstract class GenericGatherAgent
    {
        protected PublishAgent sendBoi = new PublishAgent();
        protected static string searchTerm = "";

        protected GenericGatherAgent(string keyword)
        {
            searchTerm = keyword;
        }

        //construct JSON object for each class to push to PublishAgent.cs
        ///protected abstract string OnHitCallback();
    }
}
