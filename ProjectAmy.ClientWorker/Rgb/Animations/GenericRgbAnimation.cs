using System.Threading;
using CUESDK;

namespace ProjectAmy.ClientWorker.Rgb.Animations
{
    public abstract class GenericRgbAnimation : IKeyboardRgbAnimation<TeamsAnimationData>
    {
        private readonly IRgbController _controller;
        public CorsairLedColor Backdrop { get; }

        protected GenericRgbAnimation(IRgbController controller, CorsairLedColor backdrop)
        {
            _controller = controller;
            Backdrop = backdrop;
        }

        /// <inheritdoc />
        public void Play(TeamsAnimationData animationData)
        {
            _controller.SetBackground(Backdrop);
            _controller.TypeName(animationData.Name, new CorsairLedColor { R = 255, G = 255, B = 255 });
            _controller.Update();

            Thread.Sleep(3500);
        }
    }
}