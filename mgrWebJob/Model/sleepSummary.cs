using Newtonsoft.Json;

namespace mgrWebJob
{
    class SleepSummary
    {
        [JsonProperty("calendarDate")]
        public string calendarDate { get; set; }

        [JsonProperty("durationInSeconds")]
        public int durationInSeconds { get; set; }

        [JsonProperty("deepSleepDurationInSeconds")]
        public int deepSleepDurationInSeconds { get; set; }

        [JsonProperty("lightSleepDurationInSeconds")]
        public int lightSleepDurationInSeconds { get; set; }

        [JsonProperty("remSleepInSeconds")]
        public int remSleepInSeconds { get; set; }

        [JsonProperty("awakeDurationInSeconds")]
        public int awakeDurationInSeconds { get; set; }

        public SleepSummary()
        {
            calendarDate = "";
            durationInSeconds = 0;
            deepSleepDurationInSeconds = 0;
            lightSleepDurationInSeconds = 0;
            remSleepInSeconds = 0;
            awakeDurationInSeconds = 0;
        }
    }
}
