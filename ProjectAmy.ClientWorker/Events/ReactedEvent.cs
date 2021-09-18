namespace ProjectAmy.ClientWorker.Events
{
    public class ReactedEvent
    {
        /// <summary>
        /// Can be one of <see cref="ReactionTypes"/>
        /// </summary>
        public string ReactionType { get; set; }

        /// <summary>
        /// The name of the user who triggered the reaction.
        /// </summary>
        public string UserId { get; set; }
    }
}