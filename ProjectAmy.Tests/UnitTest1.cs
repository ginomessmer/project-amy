using System;
using ProjectAmy.ClientWorker.Rgb;
using ProjectAmy.ClientWorker.Rgb.Animations;
using Xunit;

namespace ProjectAmy.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var animation = new HeartKeyboardRgbAnimation();
            animation.Play(new TeamsAnimationData("Martha"));
        }
    }
}
