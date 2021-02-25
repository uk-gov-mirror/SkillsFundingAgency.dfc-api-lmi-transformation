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
        private readonly ITransformationService transformationService;

        public LmiWebhookService(ILogger<LmiWebhookService> logger, ITransformationService transformationService)
        {
            this.logger = logger;
            this.transformationService = transformationService;
        }

        public async Task<HttpStatusCode> ProcessMessageAsync(WebhookCacheOperation webhookCacheOperation, Guid eventId, Uri url)
        {
            logger.LogInformation($"{nameof(ProcessMessageAsync)} called in {nameof(LmiWebhookService)}");

            switch (webhookCacheOperation)
            {
                case WebhookCacheOperation.CreateOrUpdate:
                    logger.LogInformation($"{nameof(WebhookCacheOperation.CreateOrUpdate)} called in {nameof(LmiWebhookService)}");
                    await transformationService.GetAndTransformAsync().ConfigureAwait(false);
                    return HttpStatusCode.OK;
                default:
                    return HttpStatusCode.OK;
            }
        }
    }
}
