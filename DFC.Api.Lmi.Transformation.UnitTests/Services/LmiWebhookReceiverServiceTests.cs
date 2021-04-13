using DFC.Api.Lmi.Transformation.Common;
using DFC.Api.Lmi.Transformation.Enums;
using DFC.Api.Lmi.Transformation.Models;
using DFC.Api.Lmi.Transformation.Models.FunctionRequestModels;
using DFC.Api.Lmi.Transformation.Services;
using FakeItEasy;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace DFC.Api.Lmi.Transformation.UnitTests.Services
{
    [Trait("Category", "LmiWebhookReceiverService - service Unit Tests")]
    public class LmiWebhookReceiverServiceTests
    {
        protected const string EventTypePublished = "published";
        protected const string EventTypeDeleted = "deleted";

        private readonly ILogger<LmiWebhookReceiverService> fakeLogger = A.Fake<ILogger<LmiWebhookReceiverService>>();
        private readonly LmiWebhookReceiverService lmiWebhookReceiverService;

        public LmiWebhookReceiverServiceTests()
        {
            lmiWebhookReceiverService = new LmiWebhookReceiverService(fakeLogger);
        }

        [Theory]
        [InlineData(null, MessageContentType.None)]
        [InlineData("", MessageContentType.None)]
        [InlineData("https://somewhere.com/api/" + Constants.ApiForJobGroups, MessageContentType.JobGroup)]
        [InlineData("https://somewhere.com/api/" + Constants.ApiForJobGroups + "/", MessageContentType.JobGroupItem)]
        public void LmiWebhookReceiverServiceDetermineMessageContentTypeReturnsExpected(string? apiEndpoint, MessageContentType expectedResult)
        {
            // Arrange

            // Act
            var result = LmiWebhookReceiverService.DetermineMessageContentType(apiEndpoint);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(MessageContentType.None, WebhookCacheOperation.None, WebhookCommand.None)]
        [InlineData(MessageContentType.None, WebhookCacheOperation.CreateOrUpdate, WebhookCommand.None)]
        [InlineData(MessageContentType.None, WebhookCacheOperation.Delete, WebhookCommand.None)]
        [InlineData(MessageContentType.JobGroup, WebhookCacheOperation.None, WebhookCommand.None)]
        [InlineData(MessageContentType.JobGroup, WebhookCacheOperation.CreateOrUpdate, WebhookCommand.TransformAllSocToJobGroup)]
        [InlineData(MessageContentType.JobGroup, WebhookCacheOperation.Delete, WebhookCommand.PurgeAllJobGroups)]
        [InlineData(MessageContentType.JobGroupItem, WebhookCacheOperation.None, WebhookCommand.None)]
        [InlineData(MessageContentType.JobGroupItem, WebhookCacheOperation.CreateOrUpdate, WebhookCommand.TransformSocToJobGroup)]
        [InlineData(MessageContentType.JobGroupItem, WebhookCacheOperation.Delete, WebhookCommand.PurgeJobGroup)]
        public void LmiWebhookReceiverServiceDetermineWebhookCommandReturnsExpected(MessageContentType messageContentType, WebhookCacheOperation webhookCacheOperation, WebhookCommand expectedResult)
        {
            // Arrange

            // Act
            var result = LmiWebhookReceiverService.DetermineWebhookCommand(messageContentType, webhookCacheOperation);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventReturnsExpectedSubscriptionRequest()
        {
            // Arrange
            var subscriptionValidationEventData = new SubscriptionValidationEventData("a validation code", "a validation url");
            var expectedResult = new WebhookRequestModel
            {
                WebhookCommand = WebhookCommand.SubscriptionValidation,
                SubscriptionValidationResponse = new SubscriptionValidationResponse { ValidationResponse = subscriptionValidationEventData.ValidationCode },
            };
            var eventGridEvents = BuildValidEventGridEvent(Microsoft.Azure.EventGrid.EventTypes.EventGridSubscriptionValidationEvent, subscriptionValidationEventData);
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var result = lmiWebhookReceiverService.ExtractEvent(requestBody);

            // Assert
            Assert.Equal(expectedResult.WebhookCommand, result.WebhookCommand);
            Assert.Equal(expectedResult.SubscriptionValidationResponse.ValidationResponse, result.SubscriptionValidationResponse?.ValidationResponse);
        }

        [Theory]
        [InlineData(EventTypePublished, WebhookCommand.TransformAllSocToJobGroup, "https://somewhere.com/api/" + Constants.ApiForJobGroups)]
        [InlineData(EventTypePublished, WebhookCommand.TransformSocToJobGroup, "https://somewhere.com/api/" + Constants.ApiForJobGroups + "/")]
        [InlineData(EventTypeDeleted, WebhookCommand.PurgeAllJobGroups, "https://somewhere.com/api/" + Constants.ApiForJobGroups)]
        [InlineData(EventTypeDeleted, WebhookCommand.PurgeJobGroup, "https://somewhere.com/api/" + Constants.ApiForJobGroups + "/")]
        public void LmiWebhookReceiverServiceExtractEventReturnsExpected(string eventType, WebhookCommand webhookCommand, string api)
        {
            // Arrange
            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = api,
            };
            var eventGridEvents = BuildValidEventGridEvent(eventType, eventGridEventData);
            var expectedResult = new WebhookRequestModel
            {
                WebhookCommand = webhookCommand,
                EventId = Guid.Parse(eventGridEvents.First().Id),
                EventType = eventGridEvents.First().EventType,
                ContentId = Guid.Parse(eventGridEventData.ItemId),
                Url = new Uri(eventGridEventData.Api, UriKind.Absolute),
                SubscriptionValidationResponse = null,
            };
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var result = lmiWebhookReceiverService.ExtractEvent(requestBody);

            // Assert
            Assert.Equal(expectedResult.WebhookCommand, result.WebhookCommand);
            Assert.Equal(expectedResult.EventType, result.EventType);
            Assert.Equal(expectedResult.ContentId, result.ContentId);
            Assert.Equal(expectedResult.Url, result.Url);
            Assert.Null(result.SubscriptionValidationResponse?.ValidationResponse);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventRaisesExceptionForInvalidEventId()
        {
            // Arrange
            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = "https://somewhere.com",
            };
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, eventGridEventData);
            eventGridEvents.First().Id = string.Empty;
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var exceptionResult = Assert.Throws<InvalidDataException>(() => lmiWebhookReceiverService.ExtractEvent(requestBody));

            // Assert
            Assert.Equal($"Invalid Guid for EventGridEvent.Id '{eventGridEvents.First().Id}'", exceptionResult.Message);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventRaisesExceptionForInvalidItemId()
        {
            // Arrange
            var eventGridEventData = new EventGridEventData
            {
                ItemId = string.Empty,
                Api = "https://somewhere.com",
            };
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, eventGridEventData);
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var exceptionResult = Assert.Throws<InvalidDataException>(() => lmiWebhookReceiverService.ExtractEvent(requestBody));

            // Assert
            Assert.Equal($"Invalid Guid for EventGridEvent.Data.ItemId '{eventGridEventData.ItemId}'", exceptionResult.Message);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventRaisesExceptionForInvalidApi()
        {
            // Arrange
            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = "https:somewhere.com",
            };
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, eventGridEventData);
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var exceptionResult = Assert.Throws<InvalidDataException>(() => lmiWebhookReceiverService.ExtractEvent(requestBody));

            // Assert
            Assert.Equal($"Invalid Api url '{eventGridEventData.Api}' received for Event Id: {eventGridEvents.First().Id}", exceptionResult.Message);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventReturnsNone()
        {
            // Arrange
            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = "https://somewhere.com/api/",
            };
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, eventGridEventData);
            var expectedResult = new WebhookRequestModel
            {
                WebhookCommand = WebhookCommand.None,
            };
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var result = lmiWebhookReceiverService.ExtractEvent(requestBody);

            // Assert
            Assert.Equal(expectedResult.WebhookCommand, result.WebhookCommand);
        }

        [Fact]
        public void LmiWebhookReceiverServiceExtractEventRaisesExceptionForEventData()
        {
            // Arrange
            EventGridEventData? nullEventGridEventData = null;
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, nullEventGridEventData);
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var exceptionResult = Assert.Throws<InvalidDataException>(() => lmiWebhookReceiverService.ExtractEvent(requestBody));

            // Assert
            Assert.Equal($"Invalid event type '{eventGridEvents.First().EventType}' received for Event Id: {eventGridEvents.First().Id}, should be one of 'draft,published,draft-discarded,unpublished,deleted'", exceptionResult.Message);
        }

        private static EventGridEvent[] BuildValidEventGridEvent<TModel>(string eventType, TModel? data)
            where TModel : class
        {
            var models = new EventGridEvent[]
            {
                new EventGridEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Subject = "a-subject",
                    Data = data,
                    EventType = eventType,
                    EventTime = DateTime.Now,
                    DataVersion = "1.0",
                },
            };

            return models;
        }
    }
}
