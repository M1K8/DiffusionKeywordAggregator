using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PushTechnology.ClientInterface.Client.Callbacks;
using PushTechnology.ClientInterface.Client.Factories;
using PushTechnology.ClientInterface.Client.Features;
using PushTechnology.ClientInterface.Client.Features.Control;
using PushTechnology.ClientInterface.Client.Features.Control.Clients;
using PushTechnology.ClientInterface.Client.Features.Control.Topics;
using PushTechnology.ClientInterface.Client.Features.TimeSeries;
using PushTechnology.ClientInterface.Client.Features.Topics;
using PushTechnology.ClientInterface.Client.Security.Authentication;
using PushTechnology.ClientInterface.Client.Session;
using PushTechnology.ClientInterface.Client.Topics;
using PushTechnology.ClientInterface.Client.Topics.Details;
using PushTechnology.ClientInterface.Data.JSON;
using PushTechnology.DiffusionCore.Client.Types;

namespace DiffusionKeywordAggregator
{
    class SubscriberAgent


    /*
     JSON Obj Schema(ish)
     {
        "keyword" = stra
        "website" = reddit || twitter
        "occurences" : n >= 0
        "start_tstamp" :
        "end_tstamp" :
     }

    then, where str1 = str2

    {
       "keyword" = str
       "total_occurences" = n + n
       "start_tstamp" :
       "end_tstamp" :  rounded the the median or both to account for slight timing drifts 
    */
    {
        static async void asyncAuth()
        {
            var authDaemon = new AuthenticationD();
            await authDaemon.Run(new CancellationToken(), "ws://localhost:8080");
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            //set up auth 
            asyncAuth();

            Console.WriteLine("Done Auth!");

            var session = Diffusion.Sessions.Principal("client").Password("password").Open("ws://localhost:8080");

            List<string> listofWordsThatActuallyHaveLittleSignificanceButAreRandomEnoughNotToGoOverDesignantedApiCallsPerHour = new List<String>
            {
                "Zealiostotle",
                "omegaLUL",
                "yeet",
                "Modern Warfare"
            };

            //session.TopicControl.RemoveTopicsAsync("the");
            
            session.Topics.AddTimeSeriesStream(listofWordsThatActuallyHaveLittleSignificanceButAreRandomEnoughNotToGoOverDesignantedApiCallsPerHour[2] , new JSONStream());

            session.Topics.SubscribeAsync(listofWordsThatActuallyHaveLittleSignificanceButAreRandomEnoughNotToGoOverDesignantedApiCallsPerHour[2] ).Wait();


            Console.WriteLine("Subbed");

            Thread.Sleep(Timeout.Infinite);

            session.Close();
        }

        public sealed class AuthenticationD
        {

            public async Task Run(CancellationToken cancellationToken, string serverUrl)
            {
                // Connect as a control session
                var session = Diffusion.Sessions.Principal("admin").Password("password").Open(serverUrl);

                try
                {
                    Task auth = session.AuthenticationControl.SetAuthenticationHandlerAsync(
                        "before-system-handler", new Auth(), cancellationToken);

                    await auth;

                    Task perm = session.TopicControl.AddMissingTopicHandlerAsync("*.*", new MissingTopicH());


                    await perm;

                    session.Close();


                }
                catch (Exception)
                {
                    session.Close();
                }
                finally
                {
                    session.Close();
                }
            }
        }

        sealed class MissingTopicH : IMissingTopicNotificationStream
        {
            static Dictionary<String, String> vals = new Dictionary<string, string>();
            sealed class PropCallback : ISessionPropertiesCallback
            {
                public void OnError(ErrorReason errorReason)
                {
                    Console.WriteLine("no");
                }

                public void OnReply(ISessionId sessionId, Dictionary<string, string> properties)
                {
                    vals = properties;
                }

                public void OnUnknownSession(ISessionId sessionId)
                {
                    Console.WriteLine("no");
                }
            }

            public async void OnMissingTopic(IMissingTopicNotification notification)
            {
                //check our user has credentials to create a new topic 


                //open up a control session
                var session = Diffusion.Sessions.Principal("admin").Password("password").Open("ws://localhost:8080");
                var topicControl = session.TopicControl;

                List<String> strL = new List<String>
                {
                    SessionProperty.PRINCIPAL
                };

                session.ClientControl.GetSessionProperties(notification.SessionId, strL, new PropCallback());

                if (vals != null && vals.Count > 0)
                {
                    switch (vals[SessionProperty.PRINCIPAL])
                    {
                        case "admin":
                            await topicControl.AddTopicAsync(notification.TopicPath, TopicType.JSON);
                            notification.Proceed();
                            break;
                        default:
                            notification.Cancel();
                            break;
                    }

                }
                session.Close();
            }

            public void OnClose()
            {
                Console.WriteLine("no");
            }

            public void OnError(ErrorReason errorReason)
            {
                Console.WriteLine(errorReason);
            }
        }

        sealed class Auth : IControlAuthenticator
        {
            public void Authenticate(string principal, ICredentials credentials, IReadOnlyDictionary<string, string> sessionProperties, IReadOnlyDictionary<string, string> proposedProperties, IAuthenticatorCallback callback)
            {
                switch (principal)
                {
                    case "admin":
                        {
                            callback.Allow(proposedProperties);
                            break;
                        }
                    case "client":
                        {
                            callback.Allow();
                            break;
                        }
                    case "block":
                        {
                            callback.Deny();
                            break;
                        }
                    default:
                        {
                            callback.Abstain();
                            break;
                        }
                }
            }

            public void OnClose() => Console.WriteLine("Closed");

            public void OnError(ErrorReason errorReason) => Console.WriteLine($"Error {errorReason}");
        }

        private sealed class JSONStream : IValueStream<IEvent<IJSON>>
        {

            public void OnClose()
                => Console.WriteLine("The subscrption stream is now closed.");

            public void OnError(ErrorReason errorReason)
                => Console.WriteLine($"An error has occured : {errorReason}.");

 
            public void OnSubscription(string topicPath, ITopicSpecification specification)
                => Console.WriteLine($"Client subscribed to {topicPath}.");

            public void OnUnsubscription(string topicPath, ITopicSpecification specification, TopicUnsubscribeReason reason)
                => Console.WriteLine($"Client unsubscribed from {topicPath} : {reason}.");


            public void OnValue(string topicPath, ITopicSpecification specification, IEvent<IJSON> oldValue, IEvent<IJSON> newValue)
            {
                //put JSON processing here
                Console.WriteLine($"New value of {topicPath} is {newValue.Value.ToJSONString()}.");

                //var d = Diffusion.DataTypes.JSON.FromJSONString(newValue);
            }
        }
    }

}
