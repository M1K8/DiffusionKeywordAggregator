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
using PushTechnology.ClientInterface.Client.Security.Authentication;
using PushTechnology.ClientInterface.Client.Session;
using PushTechnology.ClientInterface.Client.Topics;
using PushTechnology.DiffusionCore.Client.Types;

namespace DiffusionKeywordAggregator
{
    class SubscriberAgent
    {
        static async void asyncAuth()
        {
            var authDaemon = new AuthenticationD();
            await authDaemon.Run(new CancellationToken(), "ws://localhost:8080");
        }

        static void Main(string[] args)
        {
            asyncAuth();

            //TODO : set principal to var to demonstrate auth
            var session = Diffusion.Sessions
                .Principal("client")
                .Password("password")
                .Open("ws://localhost:8080");
        }

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
            var session = Diffusion.Sessions.Principal("control").Password("password").Open(serverUrl);

            try
            {
                await session.AuthenticationControl.SetAuthenticationHandlerAsync(
                    "before-system-handler", new Auth(), cancellationToken);

            }
            catch (TaskCanceledException)
            {
                //Task was canceled; 
            }
            finally
            {
                session.Close();
            }
        }
    }

    sealed class MissingTopicH : IMissingTopicHandler
    {
         Dictionary<String, String> vals = new Dictionary<string, string>();
        public void OnActive(string topicPath, IRegisteredHandler registeredHandler)
        {
        }

        public void OnClose(string topicPath)
        {
        }

        sealed class PropCallback : ISessionPropertiesCallback
        {
            private MissingTopicH _outer;

            public PropCallback(MissingTopicH h)
            {
                _outer = h;
            }
            public void OnError(ErrorReason errorReason)
            {
            }

            public void OnReply(ISessionId sessionId, Dictionary<string, string> properties)
            {
                _outer.vals = properties;
            }

            public void OnUnknownSession(ISessionId sessionId)
            {  
            }
        }

        public async void OnMissingTopic(IMissingTopicNotification notification)
        {
            //check our user has credentials to create a new topic 
            
            
            //open up a control session
            var session = Diffusion.Sessions.Principal("control").Password("password").Open("ws://localhost:8080");
            var topicControl = session.TopicControl;

            List<String> strL = new List<String>();
            strL.Add(SessionProperty.PRINCIPAL);

            session.ClientControl.GetSessionProperties(notification.SessionId, strL, new PropCallback(this));

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
                session.Close();
            }
            //create our topic



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
}
