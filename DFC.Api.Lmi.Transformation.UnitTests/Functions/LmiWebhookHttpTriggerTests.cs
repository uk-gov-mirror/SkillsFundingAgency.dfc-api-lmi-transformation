using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Functions;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILmiWebhookReceiverService fakeLmiWebhookReceiver = A.Fake<ILmiWebhookReceiverService>();

        [Fact]
        public async Task PostWithBodyReturnsOk()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.OK);
            var function = new LmiWebhookHttpTrigger(fakeLogger, fakeLmiWebhookReceiver);
            var request = BuildRequestWithValidBody("A webhook test");

            A.CallTo(() => fakeLmiWebhookReceiver.ReceiveEventsAsync(A<string>.Ignored)).Returns(new StatusCodeResult(200));

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeLmiWebhookReceiver.ReceiveEventsAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostNullRequestReturnsBadRequest()
        {
            // Arrange
            HttpRequest? request = null;
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.BadRequest);
            var function = new LmiWebhookHttpTrigger(fakeLogger, fakeLmiWebhookReceiver);

            A.CallTo(() => fakeLmiWebhookReceiver.ReceiveEventsAsync(A<string>.Ignored)).Returns(new StatusCodeResult(200));

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostNullRequestBodyReturnsBadRequest()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.BadRequest);
            var function = new LmiWebhookHttpTrigger(fakeLogger, fakeLmiWebhookReceiver);
            var request = new DefaultHttpRequest(new DefaultHttpContext());

            A.CallTo(() => fakeLmiWebhookReceiver.ReceiveEventsAsync(A<string>.Ignored)).Returns(new StatusCodeResult(200));

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

            // Assert
            var statusResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal(expectedResult.StatusCode, statusResult.StatusCode);
        }

        [Fact]
        public async Task PostCatchesException()
        {
            // Arrange
            var expectedResult = new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            var function = new LmiWebhookHttpTrigger(fakeLogger, fakeLmiWebhookReceiver);
            var request = BuildRequestWithValidBody("A webhook test");

            A.CallTo(() => fakeLmiWebhookReceiver.ReceiveEventsAsync(A<string>.Ignored)).Throws(new NotImplementedException());

            // Act
            var result = await function.Run(request).ConfigureAwait(false);

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
