using CUESDK;

namespace ProjectAmy.ClientWorker.Rgb.Animations
{
    public sealed class LikeKeyboardRgbAnimation : GenericRgbAnimation
    {
        public LikeKeyboardRgbAnimation(IRgbController controller)
            : base(controller, new CorsairLedColor { R = 255, G = 211})
        {
        }
    }
}