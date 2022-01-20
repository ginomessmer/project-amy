using CUESDK;
using Microsoft.Extensions.Logging;
using ProjectAmy.ClientWorker.Events;

namespace ProjectAmy.ClientWorker.Rgb.Animations
{
    public sealed class LikeKeyboardRgbAnimation : GenericRgbAnimation
    {
        public LikeKeyboardRgbAnimation(IRgbController controller, string type)
            : base(controller, new CorsairLedColor { R = 255, G = 211}, type)
        {
        }
    }
}