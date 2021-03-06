using Newtonsoft.Json;
using results_uploader.Objects.API;
using System.Collections.Generic;

namespace results_uploader.Objects.API
{
    /*
     * 
     * Classes for dealing with ChronoKeep API requests.
     * 
     */

    // Event specific requests
    public class GetEventRequest
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }
    }

    public class ModifyEventRequest
    {
        [JsonProperty("event")]
        public APIEvent Event { get; set; }
    }

    // Event Year specific requests.
    public class GetEventYearRequest
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("year")]
        public string Year { get; set; }
    }

    public class ModifyEventYearRequest
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("event_year")]
        public APIEventYear Year { get; set; }
    }

    // Result specific requests
    public class GetResultsRequest
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("year")]
        public string Year { get; set; }
    }

    public class AddResultsRequest
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }
        [JsonProperty("year")]
        public string Year { get; set; }
        [JsonProperty("results")]
        public List<APIResult> Results { get; set; }
    }
}
