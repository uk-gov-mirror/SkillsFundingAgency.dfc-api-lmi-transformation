using DFC.Api.Lmi.Transformation.Enums;
using DFC.Api.Lmi.Transformation.Models;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Contracts
{
    public interface IEventGridService
    {
        Task SendEventAsync(WebhookCacheOperation webhookCacheOperation, EventGridEventData? eventGridEventData, string? subject);

        bool IsValidEventGridClientOptions(EventGridClientOptions? eventGridClientOptions);
    }
}
