using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DurableCards
{
    public class DurableData
    {
        public JObject Item { get; set; }
        public List<JObject> Attachments { get; set; } = new List<JObject>();
    }
}
