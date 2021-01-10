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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "card")] DurableCardState<AttachmentData> request,
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
            [DurableClient] IDurableEntityClient entities,
            [DurableClient] IDurableOrchestrationClient orchestrations)
        {
            var entityId = EntityIdFromGuid(id);
            var entityState = await entities.ReadEntityStateAsync<AttachmentDurableCard>(entityId);

            if (!entityState.EntityExists)
            {
                return new NotFoundResult();
            }

            var attachment = request.ReadFormAsJObject();

            if (!await entityState.EntityState.ValidateAsync(attachment))
            {
                return new BadRequestResult();
            }

            var operations = new JsonPatchDocument();

            operations.Add("data/attachments/-", attachment);

            var entityOperation = new EntityOperation
            {
                EntityId = entityId,
                Document = operations
            };

            var instanceId = await orchestrations.StartNewAsync(nameof(PatchCard), entityOperation);
            var timeout = TimeSpan.FromMinutes(1);
            var response = await orchestrations.WaitForCompletionOrCreateCheckStatusResponseAsync(request, instanceId, timeout);

            return new RedirectResult($"/card/{id}");
        }

        [FunctionName(nameof(PatchCard))]
        public static async Task PatchCard(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var entityOperation = context.GetInput<EntityOperation>();

            await context.CallEntityAsync(entityOperation.EntityId, "Patch", entityOperation.Document);
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

    public class EntityOperation
    {
        public EntityId EntityId { get; set; }
        public JsonPatchDocument Document { get; set; }
    }
}