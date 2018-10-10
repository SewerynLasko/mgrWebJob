using Newtonsoft.Json;

namespace mgrWebJob
{
    public class Sys
    {
        [JsonProperty("sunrise")]
        public double sunrise { get; set; }

        [JsonProperty("sunset")]
        public double sunset { get; set; }
    }

    public class Clouds
    {
        [JsonProperty("all")]
        public double all { get; set; }
    }

    public class Wind
    {
        [JsonProperty("speed")]
        public double speed { get; set; }

        [JsonProperty("deg")]
        public double deg { get; set; }
    }

    public class Main
    {
        [JsonProperty("temp")]
        public double temp { get; set; }

        [JsonProperty("pressure")]
        public double pressure { get; set; }

        [JsonProperty("humidity")]
        public double humidity { get; set; }
    }

    public class TemperatureSummary
    {
        [JsonProperty("main")]
        public Main main { get; set; }

        [JsonProperty("visibility")]
        public double visibility { get; set; }

        [JsonProperty("wind")]
        public Wind wind { get; set; }

        [JsonProperty("clouds")]
        public Clouds clouds { get; set; }

        [JsonProperty("sys")]
        public Sys sys { get; set; }
    }
}
