namespace ProjectAmy.ClientWorker.Rgb
{
    public interface IKeyboardRgbAnimation<in T> where T : IAnimationData
    {
        /// <summary>
        /// Plays a RGB animation on the RGB device.
        /// </summary>
        /// <param name="animationData"></param>
        void Play(T animationData);
    }
}