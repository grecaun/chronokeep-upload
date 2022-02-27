using System;
using System.Text.Json.Serialization;

namespace results_uploader.Objects.API
{
    public class APIResult
    {
        public APIResult(string bib, string first, string last, int age, string gender,
            string ageGroupName, string distance, int chipSeconds, int chipMilliseconds,
            string segment, string location, int occurrence, int place, int agePlace,
            int genderPlace, bool finish, int type, int seconds, int milliseconds)
        {
            this.Bib = bib;
            this.First = first;
            this.Last = last;
            this.Age = age;
            this.Gender = gender;
            this.AgeGroup = ageGroupName;
            this.Distance = distance;
            this.ChipSeconds = chipSeconds;
            this.ChipMilliseconds = chipMilliseconds;
            this.Segment = segment;
            this.Location = location;
            this.Occurence = occurrence;
            this.Ranking = place;
            this.AgeRanking = agePlace;
            this.GenderRanking = genderPlace;
            this.Finish = finish;
            this.Type = type;
            this.Seconds = seconds;
            this.Milliseconds = milliseconds;
        }

        [JsonPropertyName("bib")]
        public string? Bib { get; set; }
        [JsonPropertyName("first")]
        public string? First { get; set; }
        [JsonPropertyName("last")]
        public string? Last { get; set; }
        [JsonPropertyName("age")]
        public int? Age { get; set; }
        [JsonPropertyName("gender")]
        public string? Gender { get; set; }
        [JsonPropertyName("age_group")]
        public string? AgeGroup { get; set; }
        [JsonPropertyName("distance")]
        public string? Distance { get; set; }
        [JsonPropertyName("seconds")]
        public int Seconds { get; set; }
        [JsonPropertyName("milliseconds")]
        public int Milliseconds { get; set; }
        [JsonPropertyName("chip_seconds")]
        public int ChipSeconds { get; set; }
        [JsonPropertyName("chip_milliseconds")]
        public int ChipMilliseconds { get; set; }
        [JsonPropertyName("segment")]
        public string? Segment { get; set; }
        [JsonPropertyName("location")]
        public string? Location { get; set; }
        [JsonPropertyName("occurence")]
        public int? Occurence { get; set; }
        [JsonPropertyName("ranking")]
        public int? Ranking { get; set; }
        [JsonPropertyName("age_ranking")]
        public int? AgeRanking { get; set; }
        [JsonPropertyName("gender_ranking")]
        public int? GenderRanking { get; set; }
        [JsonPropertyName("finish")]
        public bool? Finish { get; set; }
        [JsonPropertyName("type")]
        public int? Type { get; set; }

        public string ChipTime()
        {
            return Constants.Timing.ToTime(ChipSeconds, ChipMilliseconds);
        }

        public string Time()
        {
            return Constants.Timing.ToTime(Seconds, Milliseconds);
        }

        public string TypeName()
        {
            switch (this.Type)
            {
                case Constants.Timing.DISTANCE_TYPE_EARLY:
                    return "Early";
                case Constants.Timing.DISTANCE_TYPE_VIRTUAL:
                    return "Virtual";
                case Constants.Timing.DISTANCE_TYPE_UNOFFICIAL:
                    return "Unofficial";
                default:
                    return "";
            }
        }
    }
}
