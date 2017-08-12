using System;
using Newtonsoft.Json;

namespace Trello.Library
{
    public sealed class Card
    {
        public string Id { get; set; }
        [JsonProperty("closed")]
        public bool IsClosed { get; set; }
        public string Name { get; set; }
        [JsonProperty("desc")]
        public string Description { get; set; }
        [JsonProperty("idList")]
        public string ListId { get; set; }
        [JsonProperty("idMembers")]
        public string[] MemberIds { get; set; }
        public DateTime DateLastActivity { get; set; }
        public string ShortUrl { get; set; }
    }
}
