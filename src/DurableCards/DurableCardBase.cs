using AdaptiveCards;
using AdaptiveCards.Templating;
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

        public string Render()
        {
            var template = new AdaptiveCardTemplate(Template);
            var expanded = template.Expand(Data);
            var card = AdaptiveCard.FromJson(expanded);
            var renderer = new HtmlCardRenderer();
            var output = renderer.RenderCard(card.Card);

            return output.Html.ToString();
        }
    }
}
