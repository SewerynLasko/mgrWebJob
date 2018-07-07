using Newtonsoft.Json;

namespace mgrWebJob
{
    class activitySummary
    {
        [JsonProperty("startTimeInSeconds")]
        public int startTimeInSeconds { get; set; }

        [JsonProperty("durationInSeconds")]
        public int durationInSeconds { get; set; }

        [JsonProperty("averageHeartRateInBeatsPerMinute")]
        public int averageHeartRateInBeatsPerMinute { get; set; }

        [JsonProperty("maxHeartRateInBeatsPerMinute")]
        public float maxHeartRateInBeatsPerMinute { get; set; }

        //[JsonProperty("temperature")]
        //public int temperature { get; set; }

        //[JsonProperty("rating")]
        //public int rating { get; set; }

        public activitySummary()
        {
            startTimeInSeconds = 0;
            durationInSeconds = 0;
            averageHeartRateInBeatsPerMinute = 0;
            maxHeartRateInBeatsPerMinute = 0;
        }
    }
}
