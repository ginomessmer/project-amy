using ProjectAmy.ClientWorker.Rgb;
using ProjectAmy.ClientWorker.Rgb.Animations;
using Xunit;

namespace ProjectAmy.Tests.Animations
{
    public class AnimationIntegrationTests
    {
        [Fact]
        public void HeartAnimation_WithName_Successfully()
        {
            var animation = new HeartKeyboardRgbAnimation();
            animation.Play(new TeamsAnimationData("Martha"));
        }
    }
}
