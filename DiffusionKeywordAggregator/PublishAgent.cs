﻿using PushTechnology.ClientInterface.Client.Factories;
using PushTechnology.ClientInterface.Client.Topics;
using PushTechnology.ClientInterface.Client.Topics.Details;
using PushTechnology.ClientInterface.Data.JSON;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiffusionKeywordAggregator
{
    public class PublishAgent
    {
        public string res = "";
        class Prop : ITopicSpecification
        {
            public TopicType Type { get; } = TopicType.JSON;

            private Dictionary<string, string> dict;

            public Prop(Dictionary<string,string> d)
            {
                dict = d;
            }

            public IReadOnlyDictionary<string, string> Properties => dict;


            public ITopicSpecification WithProperties(IDictionary<string, string> properties) => new Prop(new Dictionary<string,string>(properties));

            public ITopicSpecification WithProperty(string key, string value) => new Prop(
                new Dictionary<string, string>{
                {key, value}
                });
        }
        public async Task Run(string keyword, string website, int numOfHits)
        {
            var session = Diffusion.Sessions
                .Principal("admin")
                .Password("password")
                .Open("ws://localhost:8080");

            var newValue = string.Format("{{\n" +
                "\"keyword\": \"{0}\",\n" +
                "\"info\" : \n{{ \n\"count\": {2} ,\n" +
                "\"website\": \"{1}\",\n" +
                "\"timestamp\": \"{3}\" \n }}\n  " +
                "}}", keyword, website,numOfHits, ((long)(DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds) - 300).ToString() );

           

            Dictionary<string, string> dict = new Dictionary<string, string>
            {
                {TopicSpecificationProperty.TimeSeriesEventValueType, Diffusion.DataTypes.JSON.TypeName }
            };


            await session.TopicControl.AddTopicAsync(keyword, new Prop(dict), new CancellationToken()).ConfigureAwait(false);

            



            await session.TopicUpdate.SetAsync<IJSON>(keyword, Diffusion.DataTypes.JSON.FromJSONString(newValue)).ConfigureAwait(false);

            string viewMap = $"map >{keyword} to count/{keyword} as <value(/info)>";


            await session.Topics.CreateTopicViewAsync($"count/{keyword}", viewMap).ConfigureAwait(false);



            session.Close();

            Console.WriteLine(newValue);

            res = newValue + "\n";

        }
    }
}
