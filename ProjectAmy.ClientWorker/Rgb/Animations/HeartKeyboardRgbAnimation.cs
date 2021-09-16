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
        /// <inheritdoc />
        public void Play(TeamsAnimationData animationData)
        {
            ControlDevice();

            SetBackground();

            AnimateName(animationData);

            CorsairLightingSDK.SetLedsColorsFlushBuffer();

            Thread.Sleep(3500);
        }

        private static void AnimateName(TeamsAnimationData animationData)
        {
            var keyboard = GetKeyboard();

            var chars = animationData.Name
                .ToUpper()
                .ToCharArray();

            var corsairLedIds = chars
                .Select(CorsairLightingSDK.GetLedIdForKeyName);

            foreach (var id in corsairLedIds)
            {
                CorsairLightingSDK.SetLedsColorsBufferByDeviceIndex(keyboard.index, new []
                {
                    new CorsairLedColor { R = 255, G = 255, B = 255, LedId = id }
                });

                CorsairLightingSDK.SetLedsColorsFlushBuffer();
                Thread.Sleep(500);
            }
        }

        private static void SetBackground()
        {
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
            }
        }

        private static void ControlDevice()
        {
            CorsairLightingSDK.PerformProtocolHandshake();

            if (CorsairLightingSDK.GetLastError() != CorsairError.Success)
                throw new Exception("Failed to connect to iCUE");

            CorsairLightingSDK.RequestControl(CorsairAccessMode.ExclusiveLightingControl);
        }

        private static IList<(int index, CorsairDeviceInfo device)> GetDevices()
        {
            var devices = new List<(int index, CorsairDeviceInfo device)>();

            var deviceCount = CorsairLightingSDK.GetDeviceCount();
            for (var i = 0; i < deviceCount; i++)
            {
                devices.Add((i, CorsairLightingSDK.GetDeviceInfo(i)));
            }

            return devices;
        }

        private static (int index, CorsairDeviceInfo device) GetKeyboard() =>
            GetDevices().SingleOrDefault(x => x.device.Type == CorsairDeviceType.Keyboard);
    }
}