using System.Threading;

namespace DiffusionKeywordAggregator
{
    class ConsoleSender
    {
        static public void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                run(args[0]);
            }
            Thread.Sleep(Timeout.Infinite);
        }

        private static async void run(string s)
        {
            var r = new RedditAgent(s);
            var t = new TwitterAgent(s);
            var r_ = r.Run();
            var t_ = t.Run();

            await r_;
        }

    }
}
