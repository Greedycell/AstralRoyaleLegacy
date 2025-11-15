using Newtonsoft.Json;

namespace ClashRoyale.Logic.Home
{
    public class Achievement
    {
        [JsonProperty("data")]
        public int Data { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }
}