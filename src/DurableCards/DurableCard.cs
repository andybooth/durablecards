using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System.Linq;
using System.Threading.Tasks;

namespace DurableCards
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DurableCard<TData> : IDurableCard
    {       
        [JsonProperty("template")]
        public object Template { get; set; }

        [JsonProperty("schema")]
        public object Schema { get; set; }

        [JsonProperty("data")]
        public TData Data { get; set; }

        public void Patch(JsonPatchDocument operations)
        {
            operations.ApplyTo(this);
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

        public async Task<bool> IsValidAsync(JObject data)
        {
            if (Schema == null)
            {
                return true;
            }

            var schema = await JsonSchema.FromJsonAsync(Schema.ToString());
            var errors = schema.Validate(data);

            return !errors.Any();
        }
    }

    public interface IDurableCard
    {
        void Patch(JsonPatchDocument operations);
    }
}
