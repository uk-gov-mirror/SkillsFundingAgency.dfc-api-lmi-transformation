using DFC.Api.Lmi.Transformation.Common;
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
        private readonly IWebhookContentService fakeWebhookContentService = A.Fake<IWebhookContentService>();
        private readonly IWebhookDeleteService fakeWebhookDeleteService = A.Fake<IWebhookDeleteService>();
        private readonly LmiWebhookService lmiWebhookService;

        public LmiWebhookServiceTests()
        {
            lmiWebhookService = new LmiWebhookService(fakeLogger, fakeWebhookContentService, fakeWebhookDeleteService);
        }

        [Theory]
        [InlineData(null, MessageContentType.None)]
        [InlineData("", MessageContentType.None)]
        [InlineData("https://somewhere.com/api/" + Constants.ApiForJobGroups, MessageContentType.JobGroup)]
        [InlineData("https://somewhere.com/api/" + Constants.ApiForJobGroups + "/", MessageContentType.JobGroupItem)]
        public void LmiWebhookServiceDetermineMessageContentTypeReturnsExpected(string? apiEndpoint, MessageContentType expectedResponse)
        {
            // Arrange

            // Act
            var result = LmiWebhookService.DetermineMessageContentType(apiEndpoint);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task LmiWebhookServiceProcessMessageForDeleteReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var apiEndpoint = $"https://somewhere.com/api/{Constants.ApiForJobGroups}";

            A.CallTo(() => fakeWebhookDeleteService.ProcessDeleteAsync(A<Guid>.Ignored, A<Guid>.Ignored, A<MessageContentType>.Ignored)).Returns(expectedResult);

            // Act
            var result = await lmiWebhookService.ProcessMessageAsync(WebhookCacheOperation.Delete, Guid.NewGuid(), Guid.NewGuid(), new Uri(apiEndpoint, UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeWebhookContentService.ProcessContentAsync(A<Guid>.Ignored, A<MessageContentType>.Ignored, A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeWebhookDeleteService.ProcessDeleteAsync(A<Guid>.Ignored, A<Guid>.Ignored, A<MessageContentType>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookServiceProcessMessageForCreateUpdateReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var apiEndpoint = $"https://somewhere.com/api/{Constants.ApiForJobGroups}";

            A.CallTo(() => fakeWebhookContentService.ProcessContentAsync(A<Guid>.Ignored, A<MessageContentType>.Ignored, A<Uri>.Ignored)).Returns(expectedResult);

            // Act
            var result = await lmiWebhookService.ProcessMessageAsync(WebhookCacheOperation.CreateOrUpdate, Guid.NewGuid(), Guid.NewGuid(), new Uri(apiEndpoint, UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeWebhookContentService.ProcessContentAsync(A<Guid>.Ignored, A<MessageContentType>.Ignored, A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeWebhookDeleteService.ProcessDeleteAsync(A<Guid>.Ignored, A<Guid>.Ignored, A<MessageContentType>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookServiceProcessMessageForNoneReturnsBadRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var apiEndpoint = $"https://somewhere.com/api/{Constants.ApiForJobGroups}";

            // Act
            var result = await lmiWebhookService.ProcessMessageAsync(WebhookCacheOperation.None, Guid.NewGuid(), Guid.NewGuid(), new Uri(apiEndpoint, UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeWebhookContentService.ProcessContentAsync(A<Guid>.Ignored, A<MessageContentType>.Ignored, A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeWebhookDeleteService.ProcessDeleteAsync(A<Guid>.Ignored, A<Guid>.Ignored, A<MessageContentType>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiWebhookServiceProcessMessageForBadMessageContentTypeReturnsBadRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var apiEndpoint = "https://somewhere.com/api/";

            // Act
            var result = await lmiWebhookService.ProcessMessageAsync(WebhookCacheOperation.CreateOrUpdate, Guid.NewGuid(), Guid.NewGuid(), new Uri(apiEndpoint, UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeWebhookContentService.ProcessContentAsync(A<Guid>.Ignored, A<MessageContentType>.Ignored, A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeWebhookDeleteService.ProcessDeleteAsync(A<Guid>.Ignored, A<Guid>.Ignored, A<MessageContentType>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }
    }
}
