using System;
using System.Threading;
using CUESDK;

namespace ProjectAmy.ClientWorker.Rgb.Animations
{
    public sealed class HeartKeyboardRgbAnimation : IKeyboardRgbAnimation<TeamsAnimationData>
    {
        /// <inheritdoc />
        public void Play(TeamsAnimationData animationData)
        {
            CorsairLightingSDK.PerformProtocolHandshake();

            if (CorsairLightingSDK.GetLastError() != CorsairError.Success)
                throw new Exception("Failed to connect to iCUE");

            CorsairLightingSDK.RequestControl(CorsairAccessMode.ExclusiveLightingControl);

            var color = new CorsairLedColor { B = 0, G = 0, R = 255 };

            var deviceCount = CorsairLightingSDK.GetDeviceCount();
            for (var i = 0; i < deviceCount; i++)
            {
                var deviceLeds = CorsairLightingSDK.GetLedPositionsByDeviceIndex(i);
                var buffer = new CorsairLedColor[deviceLeds.NumberOfLeds];

                for (var j = 0; j < deviceLeds.NumberOfLeds; j++)
                {
                    buffer[j] = color;
                    buffer[j].LedId = deviceLeds.LedPosition[j].LedId;
                }

                CorsairLightingSDK.SetLedsColorsBufferByDeviceIndex(i, buffer);
                CorsairLightingSDK.SetLedsColorsFlushBuffer();
            }

            Thread.Sleep(5000);
        }
    }
}