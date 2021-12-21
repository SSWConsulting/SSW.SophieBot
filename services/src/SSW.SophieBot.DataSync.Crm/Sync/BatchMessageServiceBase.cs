using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SSW.SophieBot.Sync;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Sync
{
    public abstract class BatchMessageServiceBase<TMessage, TOptions> : IBatchMessageService<TMessage, TOptions>
    {
        protected virtual ServiceBusClient ServiceBusClient { get; }

        protected virtual ILogger<BatchMessageServiceBase<TMessage, TOptions>> Logger { get; }

        public BatchMessageServiceBase(
            ServiceBusClient serviceBusClient,
            ILogger<BatchMessageServiceBase<TMessage, TOptions>> logger)
        {
            ServiceBusClient = serviceBusClient;
            Logger = logger;
        }
        public abstract Task SendMessageAsync(IEnumerable<TMessage> message, TOptions options, CancellationToken cancellationToken = default);

        public virtual async Task SendBatchStartAsync(string topicName, CancellationToken cancellationToken = default)
        {
            await SendBatchModeAsync(topicName, BatchMode.BatchStart, cancellationToken);
        }

        public virtual async Task SendBatchEndAsync(string topicName, CancellationToken cancellationToken = default)
        {
            await SendBatchModeAsync(topicName, BatchMode.BatchEnd, cancellationToken);
        }

        protected virtual async Task SendBatchModeAsync(string topicName, BatchMode batchMode, CancellationToken cancellationToken = default)
        {
            await using var sender = ServiceBusClient.CreateSender(topicName);

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync(cancellationToken);
            var jsonSerializeOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            var mqMessage = batchMode == BatchMode.BatchStart ? MqMessage<TMessage>.BatchStart() : MqMessage<TMessage>.BatchEnd();

            if (!messageBatch.TryAddMessage(new ServiceBusMessage(BinaryData.FromObjectAsJson(mqMessage, jsonSerializeOptions))))
            {
                Logger.LogError("Failed to add message to batch: {Message}", mqMessage);
            }

            await sender.SendMessagesAsync(messageBatch, cancellationToken);
        }
    }
}
