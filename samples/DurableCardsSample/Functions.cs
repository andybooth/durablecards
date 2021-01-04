using DurableCards;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
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
            var card = new DurableCard();

            card.Definition.Template = JObject.Parse(json);

            var html = card.Render();

            return new HtmlResult(html);
        }

        [FunctionName(nameof(CreateCard))]
        public static async Task<string> CreateCard(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "card")] CreateCardRequest request,
            [DurableClient] IDurableEntityClient client)
        {
            var entityId = DurableCard.NewEntityId();
            
            await client.SignalEntityAsync<IDurableCard>(entityId, c => c.SetDefinition(request.Definition));

            if (request.Data != null)
            {
                await client.SignalEntityAsync<IDurableCard>(entityId, c => c.SetData(request.Data));
            }

            return entityId.EntityKey;
        }

        [FunctionName(nameof(RenderCard))]
        public static async Task<IActionResult> RenderCard(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "card/{id:guid}")] HttpRequest request, 
            Guid id,
            [DurableClient] IDurableEntityClient client)
        {
            var entityId = DurableCard.EntityIdFromGuid(id);
            var entityState = await client.ReadEntityStateAsync<DurableCard>(entityId);

            if (!entityState.EntityExists)
            {
                return new NotFoundResult();
            }

            var html = entityState.EntityState.Render();

            return new HtmlResult(html);
        }

        [FunctionName(nameof(PostCard))]
        public static async Task<IActionResult> PostCard(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "card/{id:guid}")] HttpRequest request,
            Guid id,
            [DurableClient] IDurableEntityClient client)
        {
            var entityId = DurableCard.EntityIdFromGuid(id);
            var entityState = await client.ReadEntityStateAsync<DurableCard>(entityId);

            if (!entityState.EntityExists)
            {
                return new NotFoundResult();
            }

            var attachment = request.ReadFormAsJObject();

            if (!await entityState.EntityState.Definition.IsValidAsync(attachment))
            {
                return new BadRequestResult();
            }

            await client.SignalEntityAsync<IDurableCard>(entityId, c => c.AddAttachment(attachment));

            return new JsonResult(attachment);
        }
    }

    public class CreateCardRequest
    {
        public DurableDefinition Definition { get; set; }
        public DurableData Data { get; set; }
    }

    public class DurableCard : DurableCardBase
    {
        [FunctionName(nameof(DurableCard))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
            => context.DispatchAsync<DurableCard>();

        public static EntityId NewEntityId() => new EntityId(nameof(DurableCard), Guid.NewGuid().ToString());
        public static EntityId EntityIdFromGuid(Guid guid) => new EntityId(nameof(DurableCard), guid.ToString());
    }
}