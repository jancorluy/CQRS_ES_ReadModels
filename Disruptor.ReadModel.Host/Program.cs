using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Infrastructure;
using Disruptor.ReadModel.Tests.Messages;
using EventStore.ClientAPI;
using SimpleCQRS;

namespace Disruptor.ReadModel.Host
{
    class Program
    {

        private static EventStoreAllCatchUpSubscription _subscription;
        private static CatchUpSubscriptionSettings _catchUpSubscriptionSettings;
        private static IEventStoreConnection _connection;
        private static Position _currentPosition;
        private static ReadmodelPublisher _queue;

        
        static void Main(string[] args)
        {
            const string STREAM = "a_test_stream";
            const int DEFAULTPORT = 1113;

            var queue = new ReadmodelPublisher();
            queue.Start(typeof(OrderCreatedEvent).Assembly);
            _queue = queue;
            //uncommet to enable verbose logging in client.
            var settings = ConnectionSettings.Create();//.EnableVerboseLogging().UseConsoleLogger();
       
            using (var conn = EventStoreConnection
                .Create(settings, new Uri($"tcp://admin:changeit@localhost:{DEFAULTPORT}"), "myConn"))
            {
                _connection = conn;
                _catchUpSubscriptionSettings = new CatchUpSubscriptionSettings(100, 500, false, true);
        
                conn.ConnectAsync().Wait();

                ConnectToSubscription();
            
                Console.WriteLine("waiting for events. press enter to exit");
                Console.ReadLine();
            }
        }

        private static void SubscriptionDropped(EventStoreCatchUpSubscription eventStorePersistentSubscriptionBase,
            SubscriptionDropReason subscriptionDropReason, Exception ex)
        {
            if(subscriptionDropReason == SubscriptionDropReason.ConnectionClosed)
                ConnectToSubscription();
            Console.Out.WriteLine("Connection dropped reconnecting... possible double now");
        }

        private static void EventAppeared(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, ResolvedEvent resolvedEvent)
        {
            if (resolvedEvent.OriginalEvent.EventType.StartsWith("$")) return; //skip internal events
            if (resolvedEvent.OriginalEvent.Metadata == null || resolvedEvent.OriginalEvent.Metadata.Any() == false) return;
            try
            {
                if (!resolvedEvent.OriginalPosition.HasValue)
                {
                    throw new NotSupportedException($"Resolved event has no position, {resolvedEvent.DeserializeEvent()}");
                }

                _currentPosition = resolvedEvent.OriginalPosition.Value;

                var e = resolvedEvent.DeserializeEvent();
                _queue.Send(e, _currentPosition.CommitPosition, _currentPosition.PreparePosition);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Could not deserialize event {resolvedEvent.OriginalEvent.EventType}, the exception is {exception}");
            }
        }

        private static void ConnectToSubscription()
        {
            var pos = Position.Start;
            if (_connection != null)
                _subscription = _connection.SubscribeToAllFrom(
                    pos,
                    _catchUpSubscriptionSettings,
                    EventAppeared,
                    OnLiveProcessingStarted,
                    SubscriptionDropped
                );
        }

        private static void OnLiveProcessingStarted(EventStoreCatchUpSubscription subscription)
        {

        }
    }
}
