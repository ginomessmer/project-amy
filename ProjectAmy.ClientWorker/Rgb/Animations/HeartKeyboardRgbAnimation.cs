using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CUESDK;

namespace ProjectAmy.ClientWorker.Rgb.Animations
{
    public sealed class HeartKeyboardRgbAnimation : IKeyboardRgbAnimation<TeamsAnimationData>
    {
        private readonly IRgbController _controller;

        public HeartKeyboardRgbAnimation(IRgbController controller)
        {
            _controller = controller;
        }

        /// <inheritdoc />
        public void Play(TeamsAnimationData animationData)
        {
            _controller.SetBackground(new CorsairLedColor { R = 255 });
            _controller.TypeName(animationData.Name, new CorsairLedColor { R = 255, G = 255, B = 255 });
            _controller.Update();

            Thread.Sleep(3500);
        }
    }
}