using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Disruptor.ReadModel.Tests.Messages;

namespace Disruptor.ReadModel.Tests.Infrastructure
{


    public class FakeBus : ICommandSender
    {
        private readonly Dictionary<Type, List<Action<IMessage>>> _routes = new Dictionary<Type, List<Action<IMessage>>>();
        private readonly Dictionary<Type, List<Func<IMessage, Task>>> _asyncRoutes = new Dictionary<Type, List<Func<IMessage, Task>>>();

        public void RegisterHandler<T>(Action<T> handler) where T : IMessage
        {
            List<Action<IMessage>> handlers;

            if (!_routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Action<IMessage>>();
                _routes.Add(typeof(T), handlers);
            }

            handlers.Add((x => handler((T)x)));
        }
        public void RegisterAsyncHandler<T>(Func<T, Task> handler) where T : IMessage
        {
            List<Func<IMessage, Task>> handlers;

            if (!_asyncRoutes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Func<IMessage, Task>>();
                _asyncRoutes.Add(typeof(T), handlers);
            }

            handlers.Add((x => handler((T)x)));
        }


        public void Send<T>(T command) where T : ICommand
        {
            List<Action<IMessage>> handlers;

            if (_routes.TryGetValue(typeof(T), out handlers))
            {
                if (handlers.Count != 1) throw new InvalidOperationException("cannot send to more than one handler");
                handlers[0](command);
            }
            else
            {
                throw new InvalidOperationException("no handler registered");
            }
        }

        public async Task SendAsync<T>(T command) where T : ICommand
        {
            List<Func<IMessage, Task>> handlers;

            if (_asyncRoutes.TryGetValue(typeof(T), out handlers))
            {
                if (handlers.Count != 1) throw new InvalidOperationException("cannot send to more than one handler");
                await handlers[0](command);
            }
            else
            {
                throw new InvalidOperationException("no handler registered");
            }
        }


    }

    public interface Handles<T>
    {
        void Handle(T message);
    }

    public interface HandlesAsync<T>
    {
        Task HandleAsync(T message);
    }


    public interface ICommandSender
    {
        void Send<T>(T command) where T : ICommand;

    }
   
}