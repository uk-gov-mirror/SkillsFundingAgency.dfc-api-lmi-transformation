using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Enums;
using DFC.Api.Lmi.Transformation.Models;
using DFC.Api.Lmi.Transformation.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Transformation.UnitTests.Services
{
    [Trait("Category", "LmiWebhookReceiverService - service Unit Tests")]
    public class LmiWebhookReceiverServiceTests
    {
        protected const string EventTypePublished = "published";

        private readonly ILogger<LmiWebhookReceiverService> fakeLogger = A.Fake<ILogger<LmiWebhookReceiverService>>();
        private readonly ILmiWebhookService fakeLmiWebhookService = A.Fake<ILmiWebhookService>();
        private readonly LmiWebhookReceiverService lmiWebhookReceiverService;

        public LmiWebhookReceiverServiceTests()
        {
            lmiWebhookReceiverService = new LmiWebhookReceiverService(fakeLogger, fakeLmiWebhookService);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceReceiveEventsProcessEventSuccessfully()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.OK);
            var eventGridEvents = BuildValidEventGridEvent(EventTypePublished, new EventGridEventData { ItemId = Guid.NewGuid().ToString(), Api = "https://somewhere.com", });
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            A.CallTo(() => fakeLmiWebhookService.ProcessMessageAsync(A<WebhookCacheOperation>.Ignored, A<Guid>.Ignored, A<Uri>.Ignored)).Returns(HttpStatusCode.OK);

            // Act
            var result = await lmiWebhookReceiverService.ReceiveEventsAsync(requestBody).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookService.ProcessMessageAsync(A<WebhookCacheOperation>.Ignored, A<Guid>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceReceiveEventsSubscriptionValidationReturnsSuccess()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.OK);
            string expectedValidationCode = Guid.NewGuid().ToString();
            var eventGridEvents = BuildValidEventGridEvent(Microsoft.Azure.EventGrid.EventTypes.EventGridSubscriptionValidationEvent, new SubscriptionValidationEventData(expectedValidationCode, "https://somewhere.com"));
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            var result = await lmiWebhookReceiverService.ReceiveEventsAsync(requestBody).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookService.ProcessMessageAsync(A<WebhookCacheOperation>.Ignored, A<Guid>.Ignored, A<Uri>.Ignored)).MustNotHaveHappened();

            var statusResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task LmiWebhookReceiverServiceReceiveEventsErrorForUnknownEventType()
        {
            // Arrange
            var eventGridEvents = BuildValidEventGridEvent("Unknown", new EventGridEventData { ItemId = Guid.NewGuid().ToString(), Api = "https://somewhere.com", });
            var requestBody = JsonConvert.SerializeObject(eventGridEvents);

            // Act
            await Assert.ThrowsAsync<InvalidDataException>(async () => await lmiWebhookReceiverService.ReceiveEventsAsync(requestBody).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookService.ProcessMessageAsync(A<WebhookCacheOperation>.Ignored, A<Guid>.Ignored, A<Uri>.Ignored)).MustNotHaveHappened();
        }

        protected static EventGridEvent[] BuildValidEventGridEvent<TModel>(string eventType, TModel data)
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
