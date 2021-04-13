using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Enums;
using DFC.Api.Lmi.Transformation.Functions;
using DFC.Api.Lmi.Transformation.Models;
using DFC.Api.Lmi.Transformation.Models.FunctionRequestModels;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Transformation.UnitTests.Functions
{
    [Trait("Category", "LmiWebhookHttpTrigger - Http trigger tests")]
    public class LmiWebhookHttpTriggerTests
    {
        private readonly ILogger<LmiWebhookHttpTrigger> fakeLogger = A.Fake<ILogger<LmiWebhookHttpTrigger>>();
        private readonly ILmiWebhookReceiverService fakeLmiWebhookReceiverService = A.Fake<ILmiWebhookReceiverService>();
        private readonly IDurableOrchestrationClient fakeDurableOrchestrationClient = A.Fake<IDurableOrchestrationClient>();
        private readonly EnvironmentValues draftEnvironmentValues = new EnvironmentValues { EnvironmentNameApiSuffix = "(draft)" };
        private readonly EnvironmentValues publishedEnvironmentValues = new EnvironmentValues { EnvironmentNameApiSuffix = string.Empty };

        [Fact]
        public async Task LmiWebhookHttpTriggerPostForSubscriptionValidationReturnsOk()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.OK);
            var function = new LmiWebhookHttpTrigger(fakeLogger, draftEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody("a request body");
            var webhookRequestModel = new WebhookRequestModel
            {
                WebhookCommand = WebhookCommand.SubscriptionValidation,
            };

            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).Returns(webhookRequestModel);

            // Act
            var result = await function.Run(request, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<SocRequestModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Theory]
        [InlineData(WebhookCommand.TransformAllSocToJobGroup)]
        [InlineData(WebhookCommand.TransformSocToJobGroup)]
        [InlineData(WebhookCommand.PurgeAllJobGroups)]
        [InlineData(WebhookCommand.PurgeJobGroup)]
        public async Task LmiWebhookHttpTriggerPostForSubscriptionValidationReturnsExpectedResultCode(WebhookCommand webhookCommand)
        {
            // Arrange
            var expectedResult = HttpStatusCode.Accepted;
            var function = new LmiWebhookHttpTrigger(fakeLogger, draftEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody("a request body");
            var webhookRequestModel = new WebhookRequestModel
            {
                WebhookCommand = webhookCommand,
            };

            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).Returns(webhookRequestModel);
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<SocRequestModel>.Ignored)).Returns("An instance id");
            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).Returns(new AcceptedResult());

            // Act
            var result = await function.Run(request, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<SocRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();
            var statusResult = Assert.IsType<AcceptedResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Theory]
        [InlineData(WebhookCommand.TransformAllSocToJobGroup)]
        [InlineData(WebhookCommand.TransformSocToJobGroup)]
        [InlineData(WebhookCommand.PurgeAllJobGroups)]
        [InlineData(WebhookCommand.PurgeJobGroup)]
        public async Task LmiWebhookHttpTriggerPostForSubscriptionValidationReturnsBadRequestForPublishedEnvironment(WebhookCommand webhookCommand)
        {
            // Arrange
            var expectedResult = HttpStatusCode.BadRequest;
            var function = new LmiWebhookHttpTrigger(fakeLogger, publishedEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody("a request body");
            var webhookRequestModel = new WebhookRequestModel
            {
                WebhookCommand = webhookCommand,
            };

            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).Returns(webhookRequestModel);

            // Act
            var result = await function.Run(request, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<SocRequestModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task LmiWebhookHttpTriggerPostForSubscriptionValidationReturnsBadRequest()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.BadRequest);
            var function = new LmiWebhookHttpTrigger(fakeLogger, draftEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody("a request body");
            var webhookRequestModel = new WebhookRequestModel
            {
                WebhookCommand = WebhookCommand.None,
            };

            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).Returns(webhookRequestModel);

            // Act
            var result = await function.Run(request, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationClient.StartNewAsync(A<string>.Ignored, A<SocRequestModel>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationClient.CreateCheckStatusResponse(A<HttpRequest>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
            var statusResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task LmiWebhookHttpTriggerPostWithNoBodyReturnsBadRequest()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.BadRequest);
            var function = new LmiWebhookHttpTrigger(fakeLogger, draftEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody(string.Empty);

            // Act
            var result = await function.Run(request, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            var statusResult = Assert.IsType<BadRequestResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task LmiWebhookHttpTriggerPostCatchesException()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            var function = new LmiWebhookHttpTrigger(fakeLogger, draftEnvironmentValues, fakeLmiWebhookReceiverService);
            var request = BuildRequestWithValidBody("a request body");

            A.CallTo(() => fakeLmiWebhookReceiverService.ExtractEvent(A<string>.Ignored)).Throws(new Exception());

            // Act
            var result = await function.Run(request, fakeDurableOrchestrationClient).ConfigureAwait(false);

            // Assert
            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        private static HttpRequest BuildRequestWithValidBody(string bodyString)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyString)),
            };
        }
    }
}
