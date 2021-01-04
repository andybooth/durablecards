using AdaptiveCards;
using AdaptiveCards.Templating;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DurableCards
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DurableCardBase : IDurableCard
    {
        [JsonProperty("definition")]
        public DurableDefinition Definition { get; private set; } = new DurableDefinition();

        [JsonProperty("data")]
        public DurableData Data { get; private set; } = new DurableData();

        public void SetDefinition(DurableDefinition definition)
        {
            Definition = definition;
        }

        public void SetData(DurableData data)
        {
            Data = data;
        }

        public void AddAttachment(JObject attachment)
        {
            Data.Attachments.Add(attachment);
        }

        public string Render()
        {
            var template = new AdaptiveCardTemplate(Definition.Template);
            var expanded = template.Expand(Data);
            var card = AdaptiveCard.FromJson(expanded);
            var renderer = new HtmlCardRenderer();
            var output = renderer.RenderCard(card.Card);

            return output.Html.ToString();
        }
    }
}
