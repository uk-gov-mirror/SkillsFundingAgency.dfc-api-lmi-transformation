using DFC.Api.Lmi.Transformation.Common;
using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Enums;
using DFC.Api.Lmi.Transformation.Models;
using DFC.Api.Lmi.Transformation.Models.FunctionRequestModels;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace DFC.Api.Lmi.Transformation.Services
{
    public class LmiWebhookReceiverService : ILmiWebhookReceiverService
    {
        private readonly Dictionary<string, WebhookCacheOperation> acceptedEventTypes = new Dictionary<string, WebhookCacheOperation>
        {
            { "draft", WebhookCacheOperation.CreateOrUpdate },
            { "published", WebhookCacheOperation.CreateOrUpdate },
            { "draft-discarded", WebhookCacheOperation.Delete },
            { "unpublished", WebhookCacheOperation.Delete },
            { "deleted", WebhookCacheOperation.Delete },
        };

        private readonly ILogger<LmiWebhookReceiverService> logger;

        public LmiWebhookReceiverService(ILogger<LmiWebhookReceiverService> logger)
        {
            this.logger = logger;
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

        public static WebhookCommand DetermineWebhookCommand(MessageContentType messageContentType, WebhookCacheOperation webhookCacheOperation)
        {
            switch (webhookCacheOperation)
            {
                case WebhookCacheOperation.CreateOrUpdate:
                    switch (messageContentType)
                    {
                        case MessageContentType.JobGroup:
                            return WebhookCommand.TransformAllSocToJobGroup;
                        case MessageContentType.JobGroupItem:
                            return WebhookCommand.TransformSocToJobGroup;
                    }

                    break;
                case WebhookCacheOperation.Delete:
                    switch (messageContentType)
                    {
                        case MessageContentType.JobGroup:
                            return WebhookCommand.PurgeAllJobGroups;
                        case MessageContentType.JobGroupItem:
                            return WebhookCommand.PurgeJobGroup;
                    }

                    break;
            }

            return WebhookCommand.None;
        }

        public WebhookRequestModel ExtractEvent(string requestBody)
        {
            var webhookRequestModel = new WebhookRequestModel();

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

                    webhookRequestModel.WebhookCommand = WebhookCommand.SubscriptionValidation;
                    webhookRequestModel.SubscriptionValidationResponse = new SubscriptionValidationResponse()
                    {
                        ValidationResponse = subscriptionValidationEventData.ValidationCode,
                    };

                    return webhookRequestModel;
                }
                else if (eventGridEvent.Data is EventGridEventData eventGridEventData)
                {
                    if (!Guid.TryParse(eventGridEventData.ItemId, out Guid contentId))
                    {
                        throw new InvalidDataException($"Invalid Guid for EventGridEvent.Data.ItemId '{eventGridEventData.ItemId}'");
                    }

                    if (!Uri.TryCreate(eventGridEventData.Api, UriKind.Absolute, out Uri? url))
                    {
                        throw new InvalidDataException($"Invalid Api url '{eventGridEventData.Api}' received for Event Id: {eventId}");
                    }

                    var cacheOperation = acceptedEventTypes[eventGridEvent.EventType];

                    logger.LogInformation($"Got Event Id: {eventId}: {eventGridEvent.EventType}: Cache operation: {cacheOperation} {url}");

                    var messageContentType = DetermineMessageContentType(url.ToString());
                    if (messageContentType == MessageContentType.None)
                    {
                        logger.LogError($"Event Id: {eventId} got unknown message content type - {messageContentType} - {url}");
                        return webhookRequestModel;
                    }

                    webhookRequestModel.WebhookCommand = DetermineWebhookCommand(messageContentType, cacheOperation);
                    webhookRequestModel.EventId = eventId;
                    webhookRequestModel.EventType = eventGridEvent.EventType;
                    webhookRequestModel.ContentId = contentId;
                    webhookRequestModel.Url = url;
                }
                else
                {
                    throw new InvalidDataException($"Invalid event type '{eventGridEvent.EventType}' received for Event Id: {eventId}, should be one of '{string.Join(",", acceptedEventTypes.Keys)}'");
                }
            }

            return webhookRequestModel;
        }
    }
}
