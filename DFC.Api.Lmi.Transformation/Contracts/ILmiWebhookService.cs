using DFC.Api.Lmi.Transformation.Enums;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Contracts
{
    public interface ILmiWebhookService
    {
        Task<HttpStatusCode> ProcessMessageAsync(WebhookCacheOperation webhookCacheOperation, Guid eventId, Uri url);
    }
}
