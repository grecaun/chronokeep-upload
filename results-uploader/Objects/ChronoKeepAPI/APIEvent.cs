using Newtonsoft.Json;

namespace results_uploader.Objects.API
{
    public class APIEvent
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("website")]
        public string Website { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
        [JsonProperty("contact_email")]
        public string ContactEmail { get; set; }
        [JsonProperty("access_restricted")]
        public bool AccessRestricted { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
