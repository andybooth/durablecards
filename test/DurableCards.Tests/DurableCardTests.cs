using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Xunit;

namespace DurableCards.Tests
{
    public class DurableCardTests
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

            var durableCard = new DurableCard<object>();

            durableCard.Template = JObject.FromObject(template);

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
                        { "$data", "${blocks}" },
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

            var data = new
            {
                blocks = new[]
                {
                    new { Text = "1" },
                    new { Text = "2" }
                }
            };

            var durableCard = new DurableCard<object>();

            durableCard.Template = JObject.FromObject(template);
            durableCard.Data = JObject.FromObject(data);

            var html = durableCard.Render();

            Assert.Contains("<html>", html);
            Assert.Contains("<p>1</p>", html);
            Assert.Contains("<p>2</p>", html);
            Assert.Contains("<input", html);
        }

        [Fact]
        public void PatchesData()
        {
            var data = new
            {
                attachments = new List<string>()
            };
            
            var durableCard = new DurableCard<object>();

            durableCard.Data = data;

            var operations = new JsonPatchDocument();

            operations.Add("data/attachments/-", "1");
            operations.Add("data/attachments/-", "2");

            durableCard.Patch(operations);

            var expected = new string[]
            {
                "1", "2"
            };

            Assert.Equal(((dynamic)durableCard.Data).attachments, expected);
        }
    }
}
