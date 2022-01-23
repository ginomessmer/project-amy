using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CUESDK;
using Microsoft.Extensions.Logging;

namespace ProjectAmy.ClientWorker.Rgb.Animations
{
    public sealed class HeartKeyboardRgbAnimation : GenericRgbAnimation
    {
        
        public HeartKeyboardRgbAnimation(IRgbController controller, string type, ILogger logger)
            : base(controller, new CorsairLedColor { R = 255 }, type, logger)
        {
        }

    }
}