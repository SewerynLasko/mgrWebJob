using Newtonsoft.Json;

namespace mgrWebJob
{
    class ActivitySummary
    {
        [JsonProperty("startTimeInSeconds")]
        public int startTimeInSeconds { get; set; }

        [JsonProperty("durationInSeconds")]
        public int durationInSeconds { get; set; }

        [JsonProperty("averageHeartRateInBeatsPerMinute")]
        public int averageHeartRateInBeatsPerMinute { get; set; }

        [JsonProperty("maxHeartRateInBeatsPerMinute")]
        public float maxHeartRateInBeatsPerMinute { get; set; }

        public ActivitySummary()
        {
            startTimeInSeconds = 0;
            durationInSeconds = 0;
            averageHeartRateInBeatsPerMinute = 0;
            maxHeartRateInBeatsPerMinute = 0;
        }
    }
}
