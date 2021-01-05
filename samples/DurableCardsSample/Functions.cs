using DurableCards;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DurableCardsSample
{
    public static class Functions
    {
        [FunctionName(nameof(CreateCard))]
        public static async Task<string> CreateCard(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "card")] DurableCardRequest request,
            [DurableClient] IDurableEntityClient client)
        {
            var entityId = NewEntityId();
            var operations = new JsonPatchDocument();

            operations.Replace("template", request.Template);
            operations.Replace("data", request.Data);
            operations.Replace("schema", request.Schema);

            await client.SignalEntityAsync<IDurableCard>(entityId, c => c.Patch(operations));

            return entityId.EntityKey;
        }

        [FunctionName(nameof(RenderCard))]
        public static async Task<IActionResult> RenderCard(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "card/{id:guid}")] HttpRequest request, 
            Guid id,
            [DurableClient] IDurableEntityClient client)
        {
            var entityId = EntityIdFromGuid(id);
            var entityState = await client.ReadEntityStateAsync<AttachmentDurableCard>(entityId);

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
            var entityId = EntityIdFromGuid(id);
            var entityState = await client.ReadEntityStateAsync<AttachmentDurableCard>(entityId);

            if (!entityState.EntityExists)
            {
                return new NotFoundResult();
            }

            var attachment = request.ReadFormAsJObject();

            if (!await entityState.EntityState.IsValidAsync(attachment))
            {
                return new BadRequestResult();
            }

            var operations = new JsonPatchDocument();

            operations.Add("data/attachments/-", attachment);

            await client.SignalEntityAsync<IDurableCard>(entityId, c => c.Patch(operations));

            return new RedirectResult($"/card/{id}");
        }

        [FunctionName(nameof(AttachmentDurableCard))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
            => context.DispatchAsync<AttachmentDurableCard>();

        public static EntityId NewEntityId() => new EntityId(nameof(AttachmentDurableCard), Guid.NewGuid().ToString());
        public static EntityId EntityIdFromGuid(Guid guid) => new EntityId(nameof(AttachmentDurableCard), guid.ToString());
    }

    public class AttachmentData
    {
        public List<object> Attachments { get; set; }
    }

    public class AttachmentDurableCard : DurableCard<AttachmentData>
    {

    }
}