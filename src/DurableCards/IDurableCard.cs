using Newtonsoft.Json.Linq;

namespace DurableCards
{
    public interface IDurableCard
    {
        void SetDefinition(DurableDefinition definition);
        void SetData(DurableData data);
        void AddAttachment(JObject attachment);
    }
}
