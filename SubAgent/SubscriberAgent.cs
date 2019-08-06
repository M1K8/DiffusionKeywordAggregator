using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PushTechnology.ClientInterface;
using PushTechnology.ClientInterface.Client.Callbacks;
using PushTechnology.ClientInterface.Client.Factories;
using PushTechnology.ClientInterface.Client.Features;
using PushTechnology.ClientInterface.Client.Features.Control;
using PushTechnology.ClientInterface.Client.Features.Control.Clients;
using PushTechnology.ClientInterface.Client.Features.Control.Topics;
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
        "keyword" = str
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
            //set up auth 
            asyncAuth();

            var session = Diffusion.Sessions.Principal("client").Password("password").Open("ws://localhost:8080");

            List<string> listofWordsThatActuallyHaveLittleSignificanceButAreRandomEnoughNotToGoOverDesignantedApiCallsPerHour = new List<String>
            {
                "Zealiostotle",
                "Drogba",
                "omegaLUL",
                "pepega",
                "Modern Warfare "
            };

            var j = new JSONStream();

            session.Topics.AddStream(listofWordsThatActuallyHaveLittleSignificanceButAreRandomEnoughNotToGoOverDesignantedApiCallsPerHour[2], j);

            session.Topics.SubscribeAsync(listofWordsThatActuallyHaveLittleSignificanceButAreRandomEnoughNotToGoOverDesignantedApiCallsPerHour[2]).Wait();

            Thread.Sleep(TimeSpan.FromMinutes(10));

            session.Close();
        }

        public sealed class AuthenticationD
        {
            /// <summary>
            /// Runs the authenticator client example.
            /// </summary>
            /// <param name="cancellationToken">A token used to end the client example.</param>
            /// <param name="args">A single string should be used for the server url.</param>
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

        private sealed class JSONStream : IValueStream<IJSON>
        {
            /// <summary>
            /// Notification of stream being closed normally.
            /// </summary>
            public void OnClose()
                => Console.WriteLine("The subscrption stream is now closed.");

            /// <summary>
            /// Notification of a contextual error related to this callback.
            /// </summary>
            /// <remarks>
            /// Situations in which <code>OnError</code> is called include the session being closed, a communication
            /// timeout, or a problem with the provided parameters. No further calls will be made to this callback.
            /// </remarks>
            /// <param name="errorReason">Error reason.</param>
            public void OnError(ErrorReason errorReason)
                => Console.WriteLine($"An error has occured : {errorReason}.");

            /// <summary>
            /// Notification of a successful subscription.
            /// </summary>
            /// <param name="topicPath">Topic path.</param>
            /// <param name="specification">Topic specification.</param>
            public void OnSubscription(string topicPath, ITopicSpecification specification)
                => Console.WriteLine($"Client subscribed to {topicPath}.");

            /// <summary>
            /// Notification of a successful unsubscription.
            /// </summary>
            /// <param name="topicPath">Topic path.</param>
            /// <param name="specification">Topic specification.</param>
            /// <param name="reason">Error reason.</param>
            public void OnUnsubscription(string topicPath, ITopicSpecification specification, TopicUnsubscribeReason reason)
                => Console.WriteLine($"Client unsubscribed from {topicPath} : {reason}.");

            /// <summary>
            /// Topic update received.
            /// </summary>
            /// <param name="topicPath">Topic path.</param>
            /// <param name="specification">Topic specification.</param>
            /// <param name="oldValue">Value prior to update.</param>
            /// <param name="newValue">Value after update.</param>
            public void OnValue(string topicPath, ITopicSpecification specification, IJSON oldValue, IJSON newValue)
                => Console.WriteLine($"New value of {topicPath} is {newValue.ToJSONString()}.");
        }
    }

}
