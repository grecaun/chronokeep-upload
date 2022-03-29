using Newtonsoft.Json;

namespace results_uploader.Objects.API
{
    public class Result
    {
        public Result(string bib, string first, string last, int age, string gender,
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

        [JsonProperty("bib")]
        public string Bib { get; set; }
        [JsonProperty("first")]
        public string First { get; set; }
        [JsonProperty("last")]
        public string Last { get; set; }
        [JsonProperty("age")]
        public int Age { get; set; }
        [JsonProperty("gender")]
        public string Gender { get; set; }
        [JsonProperty("age_group")]
        public string AgeGroup { get; set; }
        [JsonProperty("distance")]
        public string Distance { get; set; }
        [JsonProperty("seconds")]
        public int Seconds { get; set; }
        [JsonProperty("milliseconds")]
        public int Milliseconds { get; set; }
        [JsonProperty("chip_seconds")]
        public int ChipSeconds { get; set; }
        [JsonProperty("chip_milliseconds")]
        public int ChipMilliseconds { get; set; }
        [JsonProperty("segment")]
        public string Segment { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("occurence")]
        public int Occurence { get; set; }
        [JsonProperty("ranking")]
        public int Ranking { get; set; }
        [JsonProperty("age_ranking")]
        public int AgeRanking { get; set; }
        [JsonProperty("gender_ranking")]
        public int GenderRanking { get; set; }
        [JsonProperty("finish")]
        public bool Finish { get; set; }
        [JsonProperty("type")]
        public int Type { get; set; }

        public string ChipTimeString
        {
            get => Constants.Timing.ToTime(ChipSeconds, ChipMilliseconds);
        }

        public string TimeString
        {
            get => Constants.Timing.ToTime(Seconds, Milliseconds);
        }

        public string TypeName
        {
            get => TypeConverter(this.Type);
        }

        public string TypeConverter(int type)
        {
            switch (type)
            {
                case Constants.Timing.RESULT_TYPE_DNF:
                    return "DNF";
                case Constants.Timing.RESULT_TYPE_DNS:
                    return "DNS";
                case Constants.Timing.RESULT_TYPE_EARLY:
                    return "Early";
                case Constants.Timing.RESULT_TYPE_VIRTUAL:
                    return "Virtual";
                case Constants.Timing.RESULT_TYPE_UNOFFICIAL:
                    return "Unofficial";
                default:
                    return "";
            }
        }

        public static int CompareTo(Result one, Result two)
        {
            if (one.Seconds == two.Seconds)
            {
                return one.Milliseconds.CompareTo(two.Milliseconds);
            }
            return one.Seconds.CompareTo(two.Seconds);
        }

        public APIResult toAPIResult()
        {
            return new APIResult(Bib, First, Last, Age, Gender, AgeGroup, Distance,
                ChipSeconds, ChipMilliseconds, Segment, Location, Occurence, Ranking,
                AgeRanking, GenderRanking, Finish, Type, Seconds, Milliseconds);
        }
    }
}
