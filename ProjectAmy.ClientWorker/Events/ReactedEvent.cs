using System.Text.Json.Serialization;

namespace ProjectAmy.ClientWorker.Events
{
    public class ReactedEvent
    {
        /// <summary>
        /// Can be one of <see cref="ReactionTypes"/>
        /// </summary>
        [JsonPropertyName("reactionType")]
        public string ReactionType { get; set; }

        /// <summary>
        /// The name of the user who triggered the reaction.
        /// </summary>
        [JsonPropertyName("userID")]
        public string UserId { get; set; }
    }
}