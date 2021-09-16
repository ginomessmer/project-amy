using ProjectAmy.ClientWorker.Events;

namespace ProjectAmy.ClientWorker.Rgb
{
    public sealed class TeamsAnimationData : IAnimationData
    {
        public string Name { get; set; }

        public TeamsAnimationData()
        {
        }

        public TeamsAnimationData(string name)
        {
            Name = name;
        }

        public TeamsAnimationData(ReactedEvent @event)
        {
            Name = @event.UserId;
        }
    }
}