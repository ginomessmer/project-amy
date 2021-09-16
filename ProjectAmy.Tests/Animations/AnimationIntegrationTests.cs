using Microsoft.Extensions.Logging;
using Moq;
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
            var logger = new Mock<ILogger<CorsairRgbController>>();
            var animation = new HeartKeyboardRgbAnimation(new CorsairRgbController(logger.Object));
            animation.Play(new TeamsAnimationData("Martha"));
        }
    }
}
