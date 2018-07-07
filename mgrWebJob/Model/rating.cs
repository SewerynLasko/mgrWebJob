using Newtonsoft.Json;

namespace mgrWebJob
{
    class rating
    {
        [JsonProperty("userRating")]
        public int userRating { get; set; }

        public rating()
        {
            userRating = 0;
        }
    }
}
