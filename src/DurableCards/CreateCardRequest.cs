using AdaptiveCards;
using Newtonsoft.Json.Linq;

namespace DurableCards
{
    public class CreateCardRequest
    {
        public AdaptiveCard Template { get; set; }
        public JObject Data { get; set; }
    }
}
