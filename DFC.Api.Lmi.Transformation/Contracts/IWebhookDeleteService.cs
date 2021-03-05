using DFC.Api.Lmi.Transformation.Enums;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Contracts
{
    public interface IWebhookDeleteService
    {
        Task<HttpStatusCode> ProcessDeleteAsync(Guid eventId, Guid contentId, MessageContentType messageContentType);

        Task<HttpStatusCode> PurgeSocAsync();

        Task<HttpStatusCode> DeleteSocItemAsync(Guid contentId);
    }
}
