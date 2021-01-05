using Newtonsoft.Json;

namespace DurableCards
{
    public class DurableCardState<TData>
    {
        [JsonProperty("template")]
        public object Template { get; set; }

        [JsonProperty("schema")]
        public object Schema { get; set; }

        [JsonProperty("data")]
        public TData Data { get; set; }
    }
}
