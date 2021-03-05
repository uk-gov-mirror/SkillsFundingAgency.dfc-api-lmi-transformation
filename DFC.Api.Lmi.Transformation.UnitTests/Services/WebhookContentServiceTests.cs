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
    [Trait("Category", "WebhookContentService - service Unit Tests")]
    public class WebhookContentServiceTests
    {
        private readonly ILogger<WebhookContentService> fakeLogger = A.Fake<ILogger<WebhookContentService>>();
        private readonly ITransformationService fakeTransformationService = A.Fake<ITransformationService>();
        private readonly WebhookContentService webhookContentService;

        public WebhookContentServiceTests()
        {
            webhookContentService = new WebhookContentService(fakeLogger, fakeTransformationService);
        }

        [Fact]
        public async Task WebhookContentServiceProcessContentForJobGroupIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            A.CallTo(() => fakeTransformationService.TransformAsync()).Returns(expectedResult);

            // Act
            var result = await webhookContentService.ProcessContentAsync(Guid.NewGuid(), MessageContentType.JobGroup, new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeTransformationService.TransformAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeTransformationService.TransformItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task WebhookContentServiceProcessContentForJobGroupItemIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            A.CallTo(() => fakeTransformationService.TransformItemAsync(A<Uri>.Ignored)).Returns(expectedResult);

            // Act
            var result = await webhookContentService.ProcessContentAsync(Guid.NewGuid(), MessageContentType.JobGroupItem, new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeTransformationService.TransformAsync()).MustNotHaveHappened();
            A.CallTo(() => fakeTransformationService.TransformItemAsync(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task WebhookContentServiceProcessContentForNoneReturnsBadRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;

            // Act
            var result = await webhookContentService.ProcessContentAsync(Guid.NewGuid(), MessageContentType.None, new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeTransformationService.TransformAsync()).MustNotHaveHappened();
            A.CallTo(() => fakeTransformationService.TransformItemAsync(A<Uri>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }
    }
}
