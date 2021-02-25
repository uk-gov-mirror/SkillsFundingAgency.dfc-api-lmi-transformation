using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Enums;
using DFC.Api.Lmi.Transformation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Services
{
    public class LmiWebhookReceiverService : ILmiWebhookReceiverService
    {
        private readonly Dictionary<string, WebhookCacheOperation> acceptedEventTypes = new Dictionary<string, WebhookCacheOperation>
        {
            { "draft", WebhookCacheOperation.CreateOrUpdate },
            { "published", WebhookCacheOperation.CreateOrUpdate },
            //{ "draft-discarded", WebhookCacheOperation.Delete },
            //{ "unpublished", WebhookCacheOperation.Delete },
            //{ "deleted", WebhookCacheOperation.Delete },
        };

        private readonly ILogger<LmiWebhookReceiverService> logger;
        private readonly ILmiWebhookService lmiWebhookService;

        public LmiWebhookReceiverService(
            ILogger<LmiWebhookReceiverService> logger,
            ILmiWebhookService lmiWebhookService)
        {
            this.logger = logger;
            this.lmiWebhookService = lmiWebhookService;
        }

        public async Task<IActionResult> ReceiveEventsAsync(string requestBody)
        {
            logger.LogInformation($"Received events: {requestBody}");

            var eventGridSubscriber = new EventGridSubscriber();
            foreach (var key in acceptedEventTypes.Keys)
            {
                eventGridSubscriber.AddOrUpdateCustomEventMapping(key, typeof(EventGridEventData));
            }

            var eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents(requestBody);

            foreach (var eventGridEvent in eventGridEvents)
            {
                if (!Guid.TryParse(eventGridEvent.Id, out Guid eventId))
                {
                    throw new InvalidDataException($"Invalid Guid for EventGridEvent.Id '{eventGridEvent.Id}'");
                }

                if (eventGridEvent.Data is SubscriptionValidationEventData subscriptionValidationEventData)
                {
                    logger.LogInformation($"Got SubscriptionValidation event data, validationCode: {subscriptionValidationEventData!.ValidationCode},  validationUrl: {subscriptionValidationEventData.ValidationUrl}, topic: {eventGridEvent.Topic}");

                    var responseData = new SubscriptionValidationResponse()
                    {
                        ValidationResponse = subscriptionValidationEventData.ValidationCode,
                    };

                    return new OkObjectResult(responseData);
                }
                else if (eventGridEvent.Data is EventGridEventData eventGridEventData)
                {
                    if (!Uri.TryCreate(eventGridEventData.Api, UriKind.Absolute, out Uri? url))
                    {
                        throw new InvalidDataException($"Invalid Api url '{eventGridEventData.Api}' received for Event Id: {eventId}");
                    }

                    var cacheOperation = acceptedEventTypes[eventGridEvent.EventType];

                    logger.LogInformation($"Got Event Id: {eventId}: {eventGridEvent.EventType}: Cache operation: {cacheOperation} {url}");

                    var result = await lmiWebhookService.ProcessMessageAsync(cacheOperation, eventId, url).ConfigureAwait(false);

                    LogResult(eventId, result);
                }
                else
                {
                    throw new InvalidDataException($"Invalid event type '{eventGridEvent.EventType}' received for Event Id: {eventId}, should be one of '{string.Join(",", acceptedEventTypes.Keys)}'");
                }
            }

            return new OkResult();
        }

        private void LogResult(Guid eventId, HttpStatusCode result)
        {
            switch (result)
            {
                case HttpStatusCode.OK:
                    logger.LogInformation($"Event Id: {eventId}: Replaced LMI / job-groups");
                    break;

                //case HttpStatusCode.Created:
                //    logger.LogInformation($"Event Id: {eventId}: Created LMI / job-groups");
                //    break;

                //case HttpStatusCode.AlreadyReported:
                //    logger.LogInformation($"Event Id: {eventId}: LMI / job-groups previously updated");
                //    break;

                default:
                    throw new InvalidDataException($"Event Id: {eventId}: LMI / job-groups not updated: Status: {result}");
            }
        }
    }
}
