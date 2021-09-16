using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Hosting;
using ProjectAmy.ClientWorker.Events;
using ProjectAmy.ClientWorker.Rgb;
using ProjectAmy.ClientWorker.Rgb.Animations;

namespace ProjectAmy.ClientWorker.Tasks
{
    /// <summary>
    /// Polls the queue for messages.
    /// </summary>
    public class QueuePollingWorker : BackgroundService
    {
        private readonly QueueClient _queueClient;
        private readonly IRgbController _controller;

        public QueuePollingWorker(QueueClient queueClient, IRgbController controller)
        {
            _queueClient = queueClient;
            _controller = controller;
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
                    var animation = @event.ReactionType switch
                    {
                        ReactionTypes.Heart => new HeartKeyboardRgbAnimation(_controller),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    animation.Play(new TeamsAnimationData(@event));
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}