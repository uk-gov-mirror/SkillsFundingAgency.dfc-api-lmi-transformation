using AutoMapper;
using DFC.Api.Lmi.Transformation.Functions;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Transformation.UnitTests.Functions
{
    [Trait("Category", "GetSummaryList - http trigger function Unit Tests")]
    public class GetSummaryListHttpTriggerTests
    {
        private readonly ILogger<GetSummaryListHttpTrigger> fakeLogger = A.Fake<ILogger<GetSummaryListHttpTrigger>>();
        private readonly IMapper fakeMapper = A.Fake<IMapper>();
        private readonly IDocumentService<JobGroupModel> fakeDocumentService = A.Fake<IDocumentService<JobGroupModel>>();
        private readonly GetSummaryListHttpTrigger getSummaryListHttpTrigger;

        public GetSummaryListHttpTriggerTests()
        {
            getSummaryListHttpTrigger = new GetSummaryListHttpTrigger(fakeLogger, fakeMapper, fakeDocumentService);
        }

        [Fact]
        public async Task GetSummaryListHttpTriggerRunReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var dummyModels = A.CollectionOfDummy<JobGroupModel>(2);

            A.CallTo(() => fakeDocumentService.GetAllAsync(A<string>.Ignored)).Returns(dummyModels);

            // Act
            var result = await getSummaryListHttpTrigger.Run(A.Fake<HttpRequest>()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetSummaryListHttpTriggerRunReturnsNoContent()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.NoContent;
            IList<JobGroupModel>? nullModels = default;

            A.CallTo(() => fakeDocumentService.GetAllAsync(A<string>.Ignored)).Returns(nullModels);

            // Act
            var result = await getSummaryListHttpTrigger.Run(A.Fake<HttpRequest>()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.GetAllAsync(A<string>.Ignored)).MustHaveHappenedOnceExactly();

            var statusResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal((int)expectedResult, statusResult.StatusCode);
        }
    }
}
