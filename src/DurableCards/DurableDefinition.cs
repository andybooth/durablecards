using Newtonsoft.Json.Linq;
using NJsonSchema;
using System.Linq;
using System.Threading.Tasks;

namespace DurableCards
{
    public class DurableDefinition
    {
        public JObject Template { get; set; }
        public JObject Schema { get; set; }

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
}
