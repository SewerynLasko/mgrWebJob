using Newtonsoft.Json;

namespace mgrWebJob
{
    class temperature
    {
        [JsonProperty("temperatureCelc")]
        public int temperatureCelc { get; set; }

        public temperature()
        {
            temperatureCelc = 0;
        }
    }
}
