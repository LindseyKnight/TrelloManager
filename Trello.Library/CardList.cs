using Newtonsoft.Json;

namespace Trello.Library
{
    public sealed class CardList
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonProperty("pos")]
        public float Position { get; set; }
    }
}
