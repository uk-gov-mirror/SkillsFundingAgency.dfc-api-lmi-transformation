using DFC.Api.Lmi.Transformation.Functions;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Transformation.UnitTests.Functions
{
    [Trait("Category", "GetDetai - http trigger function Unit Tests")]
    public class GetDetailHttpTriggerTests
    {
        private Guid SocId = Guid.NewGuid();

        private readonly ILogger<GetDetailHttpTrigger> fakeLogger = A.Fake<ILogger<GetDetailHttpTrigger>>();
        private readonly IDocumentService<JobGroupModel> fakeDocumentService = A.Fake<IDocumentService<JobGroupModel>>();
        private readonly GetDetailHttpTrigger getDetailHttpTrigger;

        public GetDetailHttpTriggerTests()
        {
            getDetailHttpTrigger = new GetDetailHttpTrigger(fakeLogger, fakeDocumentService);
        }

        [Fact]
        public async Task GetDetailHttpTriggerRunReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var dummyModel = A.Dummy<JobGroupModel>();

            A.CallTo(() => fakeDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).Returns(dummyModel);

            // Act
            var result = await getDetailHttpTrigger.Run(A.Fake<HttpRequest>(), SocId).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetDetailHttpTriggerRunReturnsNoContent()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            JobGroupModel? nullModel = default;

            A.CallTo(() => fakeDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).Returns(nullModel);

            // Act
            var result = await getDetailHttpTrigger.Run(A.Fake<HttpRequest>(), SocId).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetByIdAsync(A<Guid>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }
    }
}
