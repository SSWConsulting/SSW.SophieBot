using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Services
{
    public class RedisService : IDisposable
    {
        private long _lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private DateTimeOffset _firstErrorTime = DateTimeOffset.MinValue;
        private DateTimeOffset _previousErrorTime = DateTimeOffset.MinValue;

        private readonly TimeSpan ReconnectMinInterval = TimeSpan.FromSeconds(60);

        private readonly TimeSpan ReconnectErrorThreshold = TimeSpan.FromSeconds(30);
        private readonly TimeSpan RestartConnectionTimeout = TimeSpan.FromSeconds(15);
        private const int RetryMaxAttempts = 5;

        private readonly SemaphoreSlim _reconnectSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private readonly string _connectionString;
        private ConnectionMultiplexer _connection;
        private IDatabase _database;

        private RedisService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static async Task<RedisService> InitializeAsync(string connectionString)
        {
            var redisConnection = new RedisService(connectionString);
            await redisConnection.ForceReconnectAsync(initializing: true);

            return redisConnection;
        }

        public static RedisKey GetRecognizerResultKey(RecognizerResult recognizerResult)
        {
            var key = $"LuisResult.{recognizerResult.GetTopScoringIntent().intent}";
            var entities = recognizerResult.Entities;
            entities.Remove("$instance");

            return $"{key}.{entities.ToString(Formatting.None)}";
        }

        // TODO: consider using Polly. See: https://github.com/App-vNext/Polly
        public async Task<T> BasicRetryAsync<T>(Func<IDatabase, Task<T>> func)
        {
            var reconnectRetry = 0;

            while (true)
            {
                try
                {
                    return await func(_database);
                }
                catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
                {
                    reconnectRetry++;
                    if (reconnectRetry > RetryMaxAttempts)
                    {
                        throw;
                    }

                    try
                    {
                        await ForceReconnectAsync();
                    }
                    catch (ObjectDisposedException) { }
                }
            }
        }

        private async Task ForceReconnectAsync(bool initializing = false)
        {
            var previousTicks = Interlocked.Read(ref _lastReconnectTicks);
            var previousReconnectTime = new DateTimeOffset(previousTicks, TimeSpan.Zero);
            var elapsedSinceLastReconnect = DateTimeOffset.UtcNow - previousReconnectTime;

            if (elapsedSinceLastReconnect < ReconnectMinInterval)
            {
                return;
            }

            try
            {
                await _reconnectSemaphore.WaitAsync(RestartConnectionTimeout);
            }
            catch
            {
                return;
            }

            try
            {
                var utcNow = DateTimeOffset.UtcNow;
                elapsedSinceLastReconnect = utcNow - previousReconnectTime;

                if (_firstErrorTime == DateTimeOffset.MinValue && !initializing)
                {
                    _firstErrorTime = utcNow;
                    _previousErrorTime = utcNow;
                    return;
                }

                if (elapsedSinceLastReconnect < ReconnectMinInterval)
                {
                    return;
                }

                var elapsedSinceFirstError = utcNow - _firstErrorTime;
                var elapsedSinceMostRecentError = utcNow - _previousErrorTime;

                var shouldReconnect =
                    elapsedSinceFirstError >= ReconnectErrorThreshold
                    && elapsedSinceMostRecentError <= ReconnectErrorThreshold;

                _previousErrorTime = utcNow;

                if (!shouldReconnect && !initializing)
                {
                    return;
                }

                _firstErrorTime = DateTimeOffset.MinValue;
                _previousErrorTime = DateTimeOffset.MinValue;

                var oldConnection = _connection;

                if (oldConnection != null)
                {
                    try
                    {
                        await oldConnection.CloseAsync();
                    }
                    catch (Exception)
                    {
                        // Ignore any errors from the oldConnection
                    }
                }

                Interlocked.Exchange(ref _connection, null);
                ConnectionMultiplexer newConnection = await ConnectionMultiplexer.ConnectAsync(_connectionString);
                Interlocked.Exchange(ref _connection, newConnection);

                Interlocked.Exchange(ref _lastReconnectTicks, utcNow.UtcTicks);
                var newDatabase = _connection.GetDatabase();
                Interlocked.Exchange(ref _database, newDatabase);
            }
            finally
            {
                _reconnectSemaphore.Release();
            }
        }

        public void Dispose()
        {
            try { _connection?.Dispose(); } catch { }
        }
    }
}

