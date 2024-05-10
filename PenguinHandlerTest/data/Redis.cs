using System;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

public static class Redis
{
    private static ConnectionMultiplexer _connectionMultiplexer = null;

    public static void Init()
    {
        if (_connectionMultiplexer != null)
        {
            throw new InvalidOperationException("Redis is already initialized");
        }

        _connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:56471");
    }

    public static IDatabase GetDatabase()
    {
        if (_connectionMultiplexer == null)
        {
            throw new InvalidOperationException("Redis is not initialized");
        }

        return _connectionMultiplexer.GetDatabase();
    }
}