using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Services
{
    public class WebhookDeleteService : IWebhookDeleteService
    {
        private readonly ILogger<WebhookDeleteService> logger;
        private readonly ITransformationService transformationService;

        public WebhookDeleteService(
            ILogger<WebhookDeleteService> logger,
            ITransformationService transformationService)
        {
            this.logger = logger;
            this.transformationService = transformationService;
        }

        public async Task<HttpStatusCode> ProcessDeleteAsync(Guid eventId, Guid contentId, MessageContentType messageContentType)
        {
            switch (messageContentType)
            {
                case MessageContentType.JobGroup:
                    logger.LogInformation($"Event Id: {eventId} - purging LMI SOC");
                    return await PurgeSocAsync().ConfigureAwait(false);
                case MessageContentType.JobGroupItem:
                    logger.LogInformation($"Event Id: {eventId} - deleting LMI SOC item {contentId}");
                    return await DeleteSocItemAsync(contentId).ConfigureAwait(false);
            }

            return HttpStatusCode.BadRequest;
        }

        public async Task<HttpStatusCode> PurgeSocAsync()
        {
            var result = await transformationService.PurgeAsync().ConfigureAwait(false);

            return result ? HttpStatusCode.OK : HttpStatusCode.NoContent;
        }

        public async Task<HttpStatusCode> DeleteSocItemAsync(Guid contentId)
        {
            var result = await transformationService.DeleteAsync(contentId).ConfigureAwait(false);

            return result ? HttpStatusCode.OK : HttpStatusCode.NoContent;
        }
    }
}
