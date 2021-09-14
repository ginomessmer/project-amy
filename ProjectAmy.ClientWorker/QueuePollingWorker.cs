using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Hosting;
using ProjectAmy.ClientWorker.Events;

namespace ProjectAmy.ClientWorker
{
    public class QueuePollingWorker : BackgroundService
    {
        private readonly QueueClient _queueClient;

        public QueuePollingWorker(QueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!await _queueClient.ExistsAsync(stoppingToken))
                    break;

                var response = await _queueClient.ReceiveMessagesAsync(stoppingToken);
                var messages = response.Value.ToList();
                foreach (var message in messages)
                {
                    var @event = JsonSerializer.Deserialize<ReactedEvent>(message.MessageText);
                    // TODO: Queue animation
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}