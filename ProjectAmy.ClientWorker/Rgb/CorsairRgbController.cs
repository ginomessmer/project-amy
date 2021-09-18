using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CUESDK;
using Microsoft.Extensions.Logging;

namespace ProjectAmy.ClientWorker.Rgb
{
    public class CorsairRgbController : IRgbController
    {
        private readonly ILogger<CorsairRgbController> _logger;

        public CorsairRgbController(ILogger<CorsairRgbController> logger)
        {
            _logger = logger;

            ControlDevice();
        }

        /// <inheritdoc />
        public void SetBackground(CorsairLedColor color)
        {
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

        /// <inheritdoc />
        public void TypeName(string name, CorsairLedColor color, int speed = 500)
        {
            var keyboard = GetKeyboard();

            var chars = name
                .ToUpper()
                .ToCharArray();

            var corsairLedIds = chars
                .Select(CorsairLightingSDK.GetLedIdForKeyName);

            foreach (var id in corsairLedIds)
            {
                CorsairLightingSDK.SetLedsColorsBufferByDeviceIndex(keyboard.index, new[]
                {
                    new CorsairLedColor { R = 255, G = 255, B = 255, LedId = id }
                });

                CorsairLightingSDK.SetLedsColorsFlushBuffer();
                Thread.Sleep(500);
            }
        }

        /// <inheritdoc />
        public void Update() => CorsairLightingSDK.SetLedsColorsFlushBuffer();

        /// <inheritdoc />
        public void Clear()
        {
            SetBackground(new CorsairLedColor());
        }

        private void ControlDevice()
        {
            CorsairLightingSDK.PerformProtocolHandshake();

            if (CorsairLightingSDK.GetLastError() != CorsairError.Success)
            {
                var exception = new Exception("Failed to connect to iCUE");
                _logger.LogCritical(exception, "Couldn't connect to iCUE. Exiting shortly...");

                Thread.Sleep(2000);
                Environment.Exit(1);

                throw exception;
            }

            CorsairLightingSDK.RequestControl(CorsairAccessMode.ExclusiveLightingControl);
        }

        /// <summary>
        /// Returns all connected devices.
        /// </summary>
        private static IEnumerable<(int index, CorsairDeviceInfo device)> GetDevices()
        {
            var devices = new List<(int index, CorsairDeviceInfo device)>();

            var deviceCount = CorsairLightingSDK.GetDeviceCount();
            for (var i = 0; i < deviceCount; i++)
            {
                devices.Add((i, CorsairLightingSDK.GetDeviceInfo(i)));
            }

            return devices;
        }

        /// <summary>
        /// Gets the keyboard device.
        /// </summary>
        private static (int index, CorsairDeviceInfo device) GetKeyboard() =>
            GetDevices().SingleOrDefault(x => x.device.Type == CorsairDeviceType.Keyboard);
    }
}
