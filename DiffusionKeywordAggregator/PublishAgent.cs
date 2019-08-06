using PushTechnology.ClientInterface.Client.Factories;
using System;



namespace DiffusionKeywordAggregator
{
    class PublishAgent
    {
        static void Main(string[] args)
        {
            var session = Diffusion.Sessions
                .Principal("client")
                .Password("password")
                .Open("ws://localhost:8080");
        }
    }
}
