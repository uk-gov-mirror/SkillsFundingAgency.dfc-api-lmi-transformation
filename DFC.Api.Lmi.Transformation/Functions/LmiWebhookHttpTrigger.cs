using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Enums;
using DFC.Api.Lmi.Transformation.Models.FunctionRequestModels;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Functions
{
    public class LmiWebhookHttpTrigger
    {
        private readonly ILogger<LmiWebhookHttpTrigger> logger;
        private readonly ILmiWebhookReceiverService lmiWebhookReceiverService;

        public LmiWebhookHttpTrigger(
           ILogger<LmiWebhookHttpTrigger> logger,
           ILmiWebhookReceiverService lmiWebhookReceiverService)
        {
            this.logger = logger;
            this.lmiWebhookReceiverService = lmiWebhookReceiverService;
        }

        [FunctionName("LmiWebhook")]
        [Display(Name = "LMI Webhook", Description = "Receives webhook Post requests for LMI refresh.")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Page processed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "lmi/webhook")] HttpRequest? request,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            try
            {
                logger.LogInformation("Received webhook request");

                bool isDraftEnvironment = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ApiSuffix"));
                using var streamReader = new StreamReader(request?.Body!);
                var requestBody = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(requestBody))
                {
                    logger.LogError($"{nameof(request)} body is null");
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                string? instanceId = null;
                SocRequestModel? socRequest = null;
                var webhookRequestModel = lmiWebhookReceiverService.ExtractEvent(requestBody);
                switch (webhookRequestModel.WebhookCommand)
                {
                    case WebhookCommand.SubscriptionValidation:
                        return new OkObjectResult(webhookRequestModel.SubscriptionValidationResponse);
                    case WebhookCommand.TransformAllSocToJobGroup:
                        socRequest = new SocRequestModel
                        {
                            Url = webhookRequestModel.Url,
                            IsDraftEnvironment = isDraftEnvironment,
                        };
                        instanceId = await starter.StartNewAsync(nameof(LmiOrchestrationTrigger.RefreshOrchestrator), socRequest).ConfigureAwait(false);
                        break;
                    case WebhookCommand.TransformSocToJobGroup:
                        socRequest = new SocRequestModel
                        {
                            Url = webhookRequestModel.Url,
                            SocId = webhookRequestModel.ContentId,
                            IsDraftEnvironment = isDraftEnvironment,
                        };
                        instanceId = await starter.StartNewAsync(nameof(LmiOrchestrationTrigger.RefreshJobGroupOrchestrator), socRequest).ConfigureAwait(false);
                        break;
                    case WebhookCommand.PurgeAllJobGroups:
                        socRequest = new SocRequestModel
                        {
                            Url = webhookRequestModel.Url,
                            IsDraftEnvironment = isDraftEnvironment,
                        };
                        instanceId = await starter.StartNewAsync(nameof(LmiOrchestrationTrigger.PurgeOrchestrator), socRequest).ConfigureAwait(false);
                        break;
                    case WebhookCommand.PurgeJobGroup:
                        socRequest = new SocRequestModel
                        {
                            Url = webhookRequestModel.Url,
                            SocId = webhookRequestModel.ContentId,
                            IsDraftEnvironment = isDraftEnvironment,
                        };
                        instanceId = await starter.StartNewAsync(nameof(LmiOrchestrationTrigger.PurgeJobGroupOrchestrator), socRequest).ConfigureAwait(false);
                        break;
                    default:
                        return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                logger.LogInformation($"Started orchestration with ID = '{instanceId}' for SOC {socRequest?.Url}");

                return starter.CreateCheckStatusResponse(request, instanceId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}