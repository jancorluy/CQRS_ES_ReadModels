using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Extensions;
using Disruptor.ReadModel.Tests.Infrastructure;
using Disruptor.ReadModel.Tests.Infrastructure.Consumers;
using Disruptor.ReadModel.Tests.Infrastructure.FaultHandling;
using Disruptor.ReadModel.Tests.Infrastructure.Subscriptions;
using Disruptor.ReadModel.Tests.Messages;
using EventStore.ClientAPI;
using NLog;

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

           
            LogManager.Configuration = LoggingConfig.GetDefault();
            var queue = new ReadmodelPublisher();
            queue.Start(typeof(OrderCreatedEvent).Assembly);
            _queue = queue;
            //uncommet to enable verbose logging in client.
            var settings = ConnectionSettings.Create();//.EnableVerboseLogging().UseConsoleLogger();
       
            using (var conn = EventStoreConnection
                .Create(settings, new Uri($"tcp://admin:changeit@localhost:{DEFAULTPORT}"), "myConn"))
            {
                var readModelSubscription = new ReadModelSubscription().SetRetryPolicy(Retries.Fifteen, (i) => TimeSpan.FromMilliseconds(500));
                var readModelConsumer = new ReadModelConsumer(conn, queue);
                conn.ConnectAsync().Wait();
                Task.Run(async () =>
                {
                    await readModelConsumer.ConnectToSubscription(readModelSubscription);
                });
            
                Console.WriteLine("waiting for events. press enter to exit");
                Console.ReadLine();
            }
        }
    }
}
