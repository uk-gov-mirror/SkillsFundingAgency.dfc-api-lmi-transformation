using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Enums;
using DFC.Api.Lmi.Transformation.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Transformation.UnitTests.Services
{
    [Trait("Category", "LmiWebhookService - service Unit Tests")]
    public class LmiWebhookServiceTests
    {
        private readonly ILogger<LmiWebhookService> fakeLogger = A.Fake<ILogger<LmiWebhookService>>();
        private readonly ITransformationService fakeTransformationService = A.Fake<ITransformationService>();
        private readonly LmiWebhookService lmiWebhookService;

        public LmiWebhookServiceTests()
        {
            lmiWebhookService = new LmiWebhookService(fakeLogger, fakeTransformationService);
        }

        [Fact]
        public async Task LmiWebhookServiceProcessMessageForCreateUpdateReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            A.CallTo(() => fakeTransformationService.GetAndTransformAsync());

            // Act
            var result = await lmiWebhookService.ProcessMessageAsync(WebhookCacheOperation.CreateOrUpdate, Guid.NewGuid(), new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeTransformationService.GetAndTransformAsync()).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookServiceProcessMessageFornoneReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            A.CallTo(() => fakeTransformationService.GetAndTransformAsync());

            // Act
            var result = await lmiWebhookService.ProcessMessageAsync(WebhookCacheOperation.None, Guid.NewGuid(), new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeTransformationService.GetAndTransformAsync()).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }
    }
}
