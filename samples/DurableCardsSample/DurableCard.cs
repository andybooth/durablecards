using DurableCards;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;

namespace DurableCardsSample
{
    public class DurableCard : DurableCardBase
    {
        [FunctionName(nameof(DurableCard))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
            => context.DispatchAsync<DurableCard>();
    }
}
