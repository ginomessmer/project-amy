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
using ProjectAmy.ClientWorker.Services;

namespace ProjectAmy.ClientWorker.Tasks
{
    /// <summary>
    /// Polls the queue for messages.
    /// </summary>
    public class QueuePollingWorker : BackgroundService
    {
        private readonly QueueClient _queueClient;
        private readonly IRgbController _controller;
        private readonly IUserService _userService;

        public QueuePollingWorker(QueueClient queueClient, IRgbController controller,
            IUserService userService)
        {
            _queueClient = queueClient;
            _controller = controller;
            _userService = userService;
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
                    
                    var name = await _userService.GetNameAsync(@event.UserId);

                    IKeyboardRgbAnimation<TeamsAnimationData> animation = @event.ReactionType switch
                    {
                        ReactionTypes.Heart => new HeartKeyboardRgbAnimation(_controller, @event.ReactionType ),
                        ReactionTypes.Like => new LikeKeyboardRgbAnimation(_controller, @event.ReactionType),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);

                    animation.Play(new TeamsAnimationData(name));
                    animation.Dispose();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}