using AdaptiveCards;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DurableCards
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DurableCardBase : IDurableCard
    {
        [JsonProperty("template")]
        public AdaptiveCard Template { get; set; }

        [JsonProperty("data")]
        public JObject Data { get; set; }

        public void Create(CreateCardRequest request)
        {
            Template = request.Template;
            Data = request.Data;
        }
    }
}
