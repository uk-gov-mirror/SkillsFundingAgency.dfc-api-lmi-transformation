using DFC.Api.Lmi.Transformation.Enums;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Contracts
{
    public interface IWebhookContentService
    {
        Task<HttpStatusCode> ProcessContentAsync(Guid eventId, MessageContentType messageContentType, Uri url);
    }
}
