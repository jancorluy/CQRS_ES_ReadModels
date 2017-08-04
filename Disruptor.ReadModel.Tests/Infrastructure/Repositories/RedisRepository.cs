using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StackExchange.Redis;
using StackExchange.Redis.DataTypes;
using StackExchange.Redis.DataTypes.Collections;

namespace Disruptor.ReadModel.Tests.Infrastructure.Repositories
{
    public static class RedisManager
    {
        private static ConnectionMultiplexer _multiplexer;
        public static ConnectionMultiplexer Manager
        {
            get
            {
                if (_multiplexer != null)
                    return _multiplexer;
                else
                {
                    var redisConnectionString = "readmodeltests.redis.cache.windows.net:6380,password=+s7mQgveDd5JyEkd7zK0XepaZoef1wVFUUCLm0/kUg0=,ssl=True,abortConnect=False";
                    _multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
                    return _multiplexer;
                }
                
            }
        }
    }

    public class ReadModelHandler
    {
        public string ReadmodelType { get; set; }

        public long CommitPosition { get; set; }

        public long PreparePosition { get; set; }

        public DateTime LastComittedPosition { get; set; }
    }

    public class LockToken : IDisposable
    {
        public RedisValue _value;
        private readonly string _key;

        public LockToken(string key, RedisValue value)
        {
            _value = value;
            _key = key;
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await RedisManager.Manager.GetDatabase().LockReleaseAsync(_key, _value);
            }).Wait();

        }
    }

    public class RedisReadModelRepository
    {

        private RedisTypeFactory _redisTypeFactory;
        private RedisDictionary<string, ReadModelHandler> _readModelHandlers;

        public RedisReadModelRepository()
        {
            _redisTypeFactory = new RedisTypeFactory(RedisManager.Manager);
            _readModelHandlers = _redisTypeFactory.GetDictionary<string, ReadModelHandler>("ReadModelHandlersUpdated");
        }

        public async Task<LockToken> TakeLock(string key)
        {
            var redisValue = (RedisValue) Guid.NewGuid().ToString();
            var token = new LockToken(key, redisValue);
            await RedisManager.Manager.GetDatabase().LockTakeAsync(key, (RedisValue) Guid.NewGuid().ToString(),
                TimeSpan.FromMilliseconds(10));

            return token;
        }

        public Task Add(ReadModelHandler item)
        {
           
            _readModelHandlers.Add(item.ReadmodelType, item);

            return Task.FromResult(0);
        }

        public Task Update(ReadModelHandler readModelHandler)
        {
            _readModelHandlers[readModelHandler.ReadmodelType] = readModelHandler;

            return Task.FromResult(0);
        }

        public Task<ReadModelHandler> GetReadModelHandlerByType(string typeName)
        {
            var value = _readModelHandlers.ContainsKey(typeName);
            if (!value)
                return Task.FromResult(default(ReadModelHandler));
   
            return Task.FromResult(_readModelHandlers.First(kvp => kvp.Value.ReadmodelType == typeName).Value);

        }


        public Task<IEnumerable<ReadModelHandler>> GetAll()
        {
            return Task.FromResult(_readModelHandlers.Values.ToList().AsEnumerable());
        }
    }
}
