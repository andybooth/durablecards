using Newtonsoft.Json.Linq;
using Xunit;

namespace DurableCards.Tests
{
    public class DurableCardBaseTests
    {
        [Fact]
        public void RendersTemplate()
        {
            var template = new
            {
                type = "AdaptiveCard",
                version = "1.3",
                body = new JObject[]
                {
                    new JObject
                    {
                        { "type", "TextBlock" },
                        { "text", "1" }
                    },
                    new JObject
                    {
                        { "type", "Input.Text" },
                        { "id", "text" },
                        { "label", "Text" }
                    }
                }
            };

            var durableCard = new DurableCardBase();

            durableCard.Definition.Template = JObject.FromObject(template);

            var html = durableCard.Render();

            Assert.Contains("<html>", html);
            Assert.Contains("<p>1</p>", html);
            Assert.Contains("<input", html);
        }

        [Fact]
        public void RendersTemplateWithData()
        {
            var template = new
            {
                type = "AdaptiveCard",
                version = "1.3",
                body = new JObject[]
                {
                    new JObject
                    {
                        { "type", "TextBlock" },
                        { "$data", "${item.blocks}" },
                        { "text", "${text}" }
                    },
                    new JObject
                    {
                        { "type", "Input.Text" },
                        { "id", "text" },
                        { "label", "Text" }
                    }
                }
            };

            var item = new
            {
                blocks = new[]
                {
                    new { Text = "1" },
                    new { Text = "2" }
                }
            };

            var durableCard = new DurableCardBase();

            durableCard.Definition.Template = JObject.FromObject(template);
            durableCard.Data.Item = JObject.FromObject(item);

            var html = durableCard.Render();

            Assert.Contains("<html>", html);
            Assert.Contains("<p>1</p>", html);
            Assert.Contains("<p>2</p>", html);
            Assert.Contains("<input", html);
        }
    }
}
