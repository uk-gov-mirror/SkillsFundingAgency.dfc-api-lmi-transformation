using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Services
{
    public class WebhookContentService : IWebhookContentService
    {
        private readonly ILogger<WebhookContentService> logger;
        private readonly ITransformationService transformationService;

        public WebhookContentService(
            ILogger<WebhookContentService> logger,
            ITransformationService transformationService)
        {
            this.logger = logger;
            this.transformationService = transformationService;
        }

        public async Task<HttpStatusCode> ProcessContentAsync(Guid eventId, MessageContentType messageContentType, Uri url)
        {
            switch (messageContentType)
            {
                case MessageContentType.JobGroup:
                    logger.LogInformation($"Event Id: {eventId} - Refreshing LMI SOC for: {url}");
                    return await transformationService.TransformAsync().ConfigureAwait(false);
                case MessageContentType.JobGroupItem:
                    logger.LogInformation($"Event Id: {eventId} - Refreshing LMI SOC item for: {url}");
                    return await transformationService.TransformItemAsync(url).ConfigureAwait(false);
            }

            return HttpStatusCode.BadRequest;
        }
    }
}
