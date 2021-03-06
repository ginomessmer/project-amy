using CUESDK;

namespace ProjectAmy.ClientWorker.Rgb
{
    /// <summary>
    /// Controls RGB lightning for a specific OEM.
    /// </summary>
    public interface IRgbController
    {
        /// <summary>
        /// Sets <paramref name="color"/> for all RGB devices.
        /// </summary>
        /// <param name="color">The desired background color.</param>
        void SetBackground(CorsairLedColor color);

        /// <summary>
        /// Displays the <paramref name="name"/> in the desired <paramref name="color"/> like a type writer.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="color">The key color.</param>
        /// <param name="speed">The typing speed in milliseconds.</param>
        void TypeName(string name, CorsairLedColor color, int speed = 500);

        /// <summary>
        /// Applies all the controller operations to the device.
        /// </summary>
        void Update();

        /// <summary>
        /// Clears the RGB lightning across all devices.
        /// </summary>
        void Clear();

        /// <summary>
        /// Whether or not this RGB controller is connected to a keyboard.
        /// </summary>
        /// <returns>True if this controller is connected to a device. Returns False is this controller is not connected to a device.</returns>
        bool IsConnected();
    }
}