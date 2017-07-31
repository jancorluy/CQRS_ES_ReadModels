using Disruptor.Dsl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Domain;
using Disruptor.ReadModel.Tests.Domain.CommandHandlers;
using Disruptor.ReadModel.Tests.Infrastructure;
using Disruptor.ReadModel.Tests.Messages;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore = Disruptor.ReadModel.Tests.Infrastructure.EventStore;

namespace Disruptor.ReadModel.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var nodeBuilder = EmbeddedVNodeBuilder.AsSingleNode()
                .OnDefaultEndpoints()
                .RunInMemory();
            var node = nodeBuilder.Build();
            node.StartAndWaitUntilReady().Wait();

            var embeddedConn = EmbeddedEventStoreConnection.Create(node);
            embeddedConn.ConnectAsync().Wait();
            
            var bus = new FakeBus();
            var queue = new ReadmodelPublisher();
           
            var orderCreatedHandler = new CreateOrderCommandHandler(
                new Repository<Order>(
                new Disruptor.ReadModel.Tests.Infrastructure.EventStore(embeddedConn, queue)));
            var orderItemAddedHandler = new AddItemToOrderCommandHandler(
                new Repository<Order>(
                    new Disruptor.ReadModel.Tests.Infrastructure.EventStore(embeddedConn, queue)));

            bus.RegisterAsyncHandler<CreateOrderCommand>(orderCreatedHandler.HandleAsync);
            bus.RegisterAsyncHandler<AddItemToCardCommand>(orderItemAddedHandler.HandleAsync);


            TestSendCommandAndFillupReadmodel(queue,bus);
        }

        private static void TestSendCommandAndFillupReadmodel(ReadmodelPublisher queue,
            FakeBus bus)
        {
            queue.Start(typeof(Program).Assembly);

            System.Console.WriteLine("Hit any to start. Ctrl+C to quit when running.");
            System.Console.ReadKey();

            var cancellationTokenSource = new CancellationTokenSource();

            var senderTask = new Action(async () =>
            {
                var orderId = Guid.NewGuid();

                var loopIterations = 0;
                while (loopIterations < 20)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;

                    await bus.SendAsync(new CreateOrderCommand()
                    {
                        OrderId = orderId,
                        CustomerName = "Joske"
                    });

                    await bus.SendAsync(new AddItemToCardCommand()
                    {
                        OrderId = orderId,
                        Card = "test"
                    });

                    orderId = Guid.NewGuid();

                    await Task.Delay(1, cancellationTokenSource.Token);

                    loopIterations++;
                }
            });

            var tList = new List<Task>();
            for (int i = 0; i < 1; i++)
            {
                var t = new Task(senderTask, cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
                t.Start();
                tList.Add(t);
            }


            Console.WriteLine("done");
            Console.ReadKey();
            // var summary = queue.Getsummary();
            //Console.WriteLine($"summary, send {intSequence} items, handled messages summary: {summary}");
            try
            {
                cancellationTokenSource.Cancel();
                queue.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            Console.ReadKey();
        }


    }


}
