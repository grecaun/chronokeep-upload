using Newtonsoft.Json;

namespace results_uploader.Objects.API
{
    public class APIEventYear
    {
        [JsonProperty("year")]
        public string Year { get; set; }
        [JsonProperty("date_time")]
        public string DateTime { get; set; }
        [JsonProperty("live")]
        public bool Live { get; set; }
    }
}
