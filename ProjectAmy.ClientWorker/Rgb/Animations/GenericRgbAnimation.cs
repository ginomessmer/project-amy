using System.Threading;
using CUESDK;
using Microsoft.Extensions.Logging;

namespace ProjectAmy.ClientWorker.Rgb.Animations
{
    public abstract class GenericRgbAnimation : IKeyboardRgbAnimation<TeamsAnimationData>
    {
        private readonly ILogger<GenericRgbAnimation> _logger;

        private readonly string _animationName;

        private readonly IRgbController _controller;
        public CorsairLedColor Backdrop { get; }



        protected GenericRgbAnimation(IRgbController controller, CorsairLedColor backdrop,  string animationName)
        {
            _animationName = animationName;
            _controller = controller;
            Backdrop = backdrop;
        }

        /// <inheritdoc />
        public void Play(TeamsAnimationData animationData)
        {
            if(_controller.IsConnected())
            {
                _controller.SetBackground(Backdrop);
                _controller.TypeName(animationData.Name, new CorsairLedColor { R = 255, G = 255, B = 255 });
                _controller.Update();

                Thread.Sleep(3500);
            } else
            {
                _logger.LogInformation($"Received {_animationName}");
            }
            
        }
        

        /// <inheritdoc />
        public void Dispose()
        {
            _controller.Clear();
            _controller.Update();
        }
    }
}