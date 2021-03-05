using DFC.Api.Lmi.Transformation.Common;
using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Services
{
    public class LmiWebhookService : ILmiWebhookService
    {
        private readonly ILogger<LmiWebhookService> logger;
        private readonly IWebhookContentService webhookContentService;
        private readonly IWebhookDeleteService webhookDeleteService;

        public LmiWebhookService(
            ILogger<LmiWebhookService> logger,
            IWebhookContentService webhookContentService,
            IWebhookDeleteService webhookDeleteService)
        {
            this.logger = logger;
            this.webhookContentService = webhookContentService;
            this.webhookDeleteService = webhookDeleteService;
        }

        public static MessageContentType DetermineMessageContentType(string? apiEndpoint)
        {
            if (!string.IsNullOrWhiteSpace(apiEndpoint))
            {
                if (apiEndpoint.EndsWith($"/{Constants.ApiForJobGroups}", StringComparison.OrdinalIgnoreCase))
                {
                    return MessageContentType.JobGroup;
                }

                if (apiEndpoint.Contains($"/{Constants.ApiForJobGroups}/", StringComparison.OrdinalIgnoreCase))
                {
                    return MessageContentType.JobGroupItem;
                }
            }

            return MessageContentType.None;
        }

        public async Task<HttpStatusCode> ProcessMessageAsync(WebhookCacheOperation webhookCacheOperation, Guid eventId, Guid contentId, Uri url)
        {
            var messageContentType = DetermineMessageContentType(url.ToString());
            if (messageContentType == MessageContentType.None)
            {
                logger.LogError($"Event Id: {eventId} got unknown message content type - {messageContentType} - {url}");
                return HttpStatusCode.BadRequest;
            }

            switch (webhookCacheOperation)
            {
                case WebhookCacheOperation.Delete:
                    return await webhookDeleteService.ProcessDeleteAsync(eventId, contentId, messageContentType).ConfigureAwait(false);

                case WebhookCacheOperation.CreateOrUpdate:
                    logger.LogInformation($"{nameof(WebhookCacheOperation.CreateOrUpdate)} called in {nameof(LmiWebhookService)}");
                    await webhookContentService.ProcessContentAsync(eventId, messageContentType, url).ConfigureAwait(false);
                    return HttpStatusCode.OK;
            }

            logger.LogError($"Event Id: {eventId} got unknown cache operation - {webhookCacheOperation}");
            return HttpStatusCode.BadRequest;
        }
    }
}
