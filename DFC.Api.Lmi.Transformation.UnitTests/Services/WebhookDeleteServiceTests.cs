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
    [Trait("Category", "WebhookDeleteService - service Unit Tests")]
    public class WebhookDeleteServiceTests
    {
        private readonly ILogger<WebhookDeleteService> fakeLogger = A.Fake<ILogger<WebhookDeleteService>>();
        private readonly ITransformationService fakeTransformationService = A.Fake<ITransformationService>();
        private readonly WebhookDeleteService webhookDeleteService;

        public WebhookDeleteServiceTests()
        {
            webhookDeleteService = new WebhookDeleteService(fakeLogger, fakeTransformationService);
        }

        [Fact]
        public async Task WebhookDeleteServiceProcessDeleteForJobGroupIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            A.CallTo(() => fakeTransformationService.PurgeAsync()).Returns(true);

            // Act
            var result = await webhookDeleteService.ProcessDeleteAsync(Guid.NewGuid(), Guid.NewGuid(), MessageContentType.JobGroup).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeTransformationService.PurgeAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeTransformationService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task WebhookDeleteServiceProcessDeleteForJobGroupItemIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;

            A.CallTo(() => fakeTransformationService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // Act
            var result = await webhookDeleteService.ProcessDeleteAsync(Guid.NewGuid(), Guid.NewGuid(), MessageContentType.JobGroupItem).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeTransformationService.PurgeAsync()).MustNotHaveHappened();
            A.CallTo(() => fakeTransformationService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task WebhookDeleteServiceProcessDeleteForNoneReturnsBadRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;

            A.CallTo(() => fakeTransformationService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // Act
            var result = await webhookDeleteService.ProcessDeleteAsync(Guid.NewGuid(), Guid.NewGuid(), MessageContentType.None).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeTransformationService.PurgeAsync()).MustNotHaveHappened();
            A.CallTo(() => fakeTransformationService.DeleteAsync(A<Guid>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, HttpStatusCode.OK)]
        [InlineData(false, HttpStatusCode.NoContent)]
        public async Task WebhookDeleteServicePurgeSocIsSuccessful(bool purgeResult, HttpStatusCode expectedResult)
        {
            // Arrange

            A.CallTo(() => fakeTransformationService.PurgeAsync()).Returns(purgeResult);

            // Act
            var result = await webhookDeleteService.PurgeSocAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeTransformationService.PurgeAsync()).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(true, HttpStatusCode.OK)]
        [InlineData(false, HttpStatusCode.NoContent)]
        public async Task WebhookDeleteServiceDeleteDeleteSocItemIsSuccessful(bool deleteResult, HttpStatusCode expectedResult)
        {
            // Arrange

            A.CallTo(() => fakeTransformationService.DeleteAsync(A<Guid>.Ignored)).Returns(deleteResult);

            // Act
            var result = await webhookDeleteService.DeleteSocItemAsync(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeTransformationService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }
    }
}
