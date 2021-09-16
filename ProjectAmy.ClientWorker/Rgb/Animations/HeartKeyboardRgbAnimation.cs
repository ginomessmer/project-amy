using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CUESDK;

namespace ProjectAmy.ClientWorker.Rgb.Animations
{
    public sealed class HeartKeyboardRgbAnimation : GenericRgbAnimation
    {
        public HeartKeyboardRgbAnimation(IRgbController controller)
            : base(controller, new CorsairLedColor { R = 255 })
        {
        }
    }
}