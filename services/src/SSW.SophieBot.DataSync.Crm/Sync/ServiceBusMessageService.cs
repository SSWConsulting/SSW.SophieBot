using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Sync
{
    public class ServiceBusMessageService : BatchMessageServiceBase<MqMessage<Employee>, string>
    {
        public ServiceBusMessageService(
            ServiceBusClient serviceBusClient,
            ILogger<ServiceBusMessageService> logger)
            : base(serviceBusClient, logger)
        {

        }

        public override async Task SendMessageAsync(
            IEnumerable<MqMessage<Employee>> messages,
            string topicName,
            CancellationToken cancellationToken = default)
        {
            if (messages.IsNullOrEmpty())
            {
                return;
            }

            await using var sender = ServiceBusClient.CreateSender(topicName);

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync(cancellationToken);
            var jsonSerializeOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            var totalMessageCount = messages.Count();
            var addedMessageCount = totalMessageCount;
            foreach (var message in messages)
            {
                if (!messageBatch.TryAddMessage(new ServiceBusMessage(BinaryData.FromObjectAsJson(message, jsonSerializeOptions))))
                {
                    Logger.LogError("Failed to add message to batch: {Message}", message);
                    addedMessageCount--;
                }
            }

            await sender.SendMessagesAsync(messageBatch, cancellationToken);
            Logger.LogInformation($"A batch of {addedMessageCount}/{totalMessageCount} messages " +
                $"has been published to the topic {topicName}.");
        }
    }
}
