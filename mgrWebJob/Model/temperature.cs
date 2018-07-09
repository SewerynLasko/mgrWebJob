using Newtonsoft.Json;

namespace mgrWebJob
{
    public class Main
    {
        [JsonProperty("temp")]
        public double temp { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("main")]
        public Main main { get; set; }
    }
}
