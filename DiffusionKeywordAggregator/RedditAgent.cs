using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiffusionKeywordAggregator
{
    public class RedditAgent : GenericGatherAgent
    {
        private static readonly HttpClient client = new HttpClient();
        public RedditAgent(string keyword) : base(keyword)
        {
            website = "reddit";

            result = 0;

            prev = ((long) (DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds - 100 ));
        }

        public async Task Run()
        {
            while (true)
            {
                string boilerplate = "?q=" + searchTerm + "&aggs=subreddit&after=" + prev;
                result = 0;
                var postStr = "https://api.pushshift.io/reddit/submission/search/" + boilerplate;
                Task<HttpResponseMessage> responsePosts = client.GetAsync(postStr);

                var commStr = "https://api.pushshift.io/reddit/comment/search/" + boilerplate;
                Task<HttpResponseMessage> responseComments = client.GetAsync(commStr);


                HttpResponseMessage postRes = await responsePosts;
                Task<string> responsePostsBody = postRes.Content.ReadAsStringAsync();
                postRes.EnsureSuccessStatusCode();

                HttpResponseMessage commRes = await responseComments;
                Task<string> responseCommentsBody = commRes.Content.ReadAsStringAsync();
                commRes.EnsureSuccessStatusCode();





                string post = await responsePostsBody.ConfigureAwait(false);
                string comms = await responseCommentsBody.ConfigureAwait(false);


                postRes.Dispose();
                commRes.Dispose();
                responsePostsBody.Dispose();

                responseCommentsBody.Dispose();



                var jPost = JObject.Parse(post);

                var jComm = JObject.Parse(comms);

                var resultObjP = jPost["aggs"]["subreddit"];

                foreach(JObject r in resultObjP)
                {
                    result += Convert.ToInt32(r["doc_count"].ToString());
                }

                var resultObjC = jComm["aggs"]["subreddit"];

                foreach (JObject r in resultObjC)
                {
                    result += Convert.ToInt32(r["doc_count"].ToString());
                }





                await publish().ConfigureAwait(false);
                prev = ((long)(DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds) - 100);

                await Task.Delay(TimeSpan.FromMilliseconds(10000)).ConfigureAwait(false); 
            }
        }
    }
}
