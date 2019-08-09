using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiffusionKeywordAggregator
{
    class RedditAgent : GenericGatherAgent
    {
        static readonly HttpClient client = new HttpClient();
        public RedditAgent(string keyword) : base(keyword)
        {
            website = "reddit";

            result = 0;

            prev = ((long) (DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds - 300 ));
        }

        public async Task Run()
        {
            while (true)
            {
                Console.WriteLine(prev);
                string boilerplate = "?q=" + searchTerm + "&aggs=subreddit&after=" + prev;
                result = 0;
                var postStr = "https://api.pushshift.io/reddit/submission/search/" + boilerplate;
                Task<HttpResponseMessage> responsePosts = client.GetAsync(postStr);

                var commStr = "https://api.pushshift.io/reddit/comment/search/" + boilerplate;
                Task<HttpResponseMessage> responseComments = client.GetAsync(commStr);


                HttpResponseMessage postRes = await responsePosts;
                postRes.EnsureSuccessStatusCode();

                HttpResponseMessage commRes = await responseComments;
                commRes.EnsureSuccessStatusCode();


                Task<string> responsePostsBody = postRes.Content.ReadAsStringAsync();

                Task<string> responseCommentsBody = commRes.Content.ReadAsStringAsync();

                string post = await responsePostsBody;
                string comms = await responseCommentsBody;

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

                



                await publish();
                prev = ((long)(DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds) - 300);

                //Console.WriteLine("Waiting 60 seconds...");
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }
        }
    }
}
