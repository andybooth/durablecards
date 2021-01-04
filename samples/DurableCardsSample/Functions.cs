using AdaptiveCards;
using DurableCards;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DurableCardsSample
{
    public static class Functions
    {
        [FunctionName(nameof(Welcome))]
        public static IActionResult Welcome(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "welcome")] HttpRequest request)
        {
            var json = File.ReadAllText("./welcome.json");
            var card = new DurableCard
            {
                Template = AdaptiveCard.FromJson(json).Card
            };
            var html = card.Render();

            return new HtmlResult(html);
        }

        [FunctionName(nameof(CreateCard))]
        public static async Task<string> CreateCard(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "card")] CreateCardRequest request,
            [DurableClient] IDurableEntityClient client)
        {
            var entityId = new EntityId(nameof(DurableCard), Guid.NewGuid().ToString());
            
            await client.SignalEntityAsync<IDurableCard>(entityId, c => c.Create(request));

            return entityId.EntityKey;
        }

        [FunctionName(nameof(RenderCard))]
        public static async Task<IActionResult> RenderCard(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "card/{id:guid}")] HttpRequest request, 
            Guid id,
            [DurableClient] IDurableEntityClient client)
        {
            var entityId = new EntityId(nameof(DurableCard), id.ToString());
            var entityState = await client.ReadEntityStateAsync<DurableCard>(entityId);
            var html = entityState.EntityState.Render();

            return new HtmlResult(html);
        }
    }

    public class DurableCard : DurableCardBase
    {
        [FunctionName(nameof(DurableCard))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
            => context.DispatchAsync<DurableCard>();
    }

    public class HtmlResult : ContentResult
    {
        public HtmlResult(string html)
        {
            StatusCode = (int)HttpStatusCode.OK;
            Content = html;
            ContentType = MediaTypeNames.Text.Html;
        }
    }
}