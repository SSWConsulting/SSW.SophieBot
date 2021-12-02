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
    public class ServiceBusMessageService : IBatchMessageService<MqMessage<Employee>, string>, IDisposable
    {
        private readonly ServiceBusClient _serviceBusClient;
        private ServiceBusSender _currentSender;
        private readonly ILogger<ServiceBusMessageService> _logger;

        public ServiceBusMessageService(
            ServiceBusClient serviceBusClient,
            ILogger<ServiceBusMessageService> logger)
        {
            _serviceBusClient = serviceBusClient;
            _logger = logger;
        }

        public async Task SendMessageAsync(
            IEnumerable<MqMessage<Employee>> messages,
            string topicName,
            CancellationToken cancellationToken = default)
        {
            if (messages.IsNullOrEmpty())
            {
                return;
            }

            _currentSender ??= _serviceBusClient.CreateSender(topicName);

            using ServiceBusMessageBatch messageBatch = await _currentSender.CreateMessageBatchAsync(cancellationToken);
            var jsonSerializeOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            var totalMessageCount = messages.Count();
            var addedMessageCount = totalMessageCount;
            foreach (var message in messages)
            {
                if (!messageBatch.TryAddMessage(new ServiceBusMessage(BinaryData.FromObjectAsJson(message, jsonSerializeOptions))))
                {
                    _logger.LogError("Failed to add message to batch: {Message}", message);
                    addedMessageCount--;
                }
            }

            await _currentSender.SendMessagesAsync(messageBatch, cancellationToken);
            _logger.LogInformation($"A batch of {addedMessageCount}/{totalMessageCount} messages " +
                $"has been published to the topic {topicName}.");
        }

        public void Dispose()
        {
            if (_currentSender != null)
            {
                _currentSender.DisposeAsync().AsTask().Wait();
            }
        }
    }
}
