using AutoMapper;
using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Functions;
using DFC.Api.Lmi.Transformation.Models;
using DFC.Api.Lmi.Transformation.Models.ContentApiModels;
using DFC.Api.Lmi.Transformation.Models.FunctionRequestModels;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using FakeItEasy;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Transformation.UnitTests.Functions
{
    [Trait("Category", "LMI transformation orchestration trigger function Unit Tests")]
    public class LmiOrchestrationTriggerTests
    {
        private readonly ILogger<LmiOrchestrationTrigger> fakeLogger = A.Fake<ILogger<LmiOrchestrationTrigger>>();
        private readonly IMapper fakeMapper = A.Fake<IMapper>();
        private readonly IContentTypeMappingService fakeContentTypeMappingService = A.Fake<IContentTypeMappingService>();
        private readonly ICmsApiService fakeCmsApiService = A.Fake<ICmsApiService>();
        private readonly IDocumentService<JobGroupModel> fakeJobGroupDocumentService = A.Fake<IDocumentService<JobGroupModel>>();
        private readonly IEventGridService fakeEventGridService = A.Fake<IEventGridService>();
        private readonly EventGridClientOptions dummyEventGridClientOptions = A.Dummy<EventGridClientOptions>();
        private readonly IDurableOrchestrationContext fakeDurableOrchestrationContext = A.Fake<IDurableOrchestrationContext>();
        private readonly LmiOrchestrationTrigger lmiOrchestrationTrigger;

        public LmiOrchestrationTriggerTests()
        {
            lmiOrchestrationTrigger = new LmiOrchestrationTrigger(fakeLogger, fakeMapper, fakeContentTypeMappingService, fakeCmsApiService, fakeJobGroupDocumentService, fakeEventGridService, dummyEventGridClientOptions);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        public async Task LmiOrchestrationTriggerRefreshJobGroupOrchestratorIsSuccessful(HttpStatusCode transformItemResult)
        {
            // Arrange
            const bool expectedResult = true;
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { SocId = Guid.NewGuid() });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<HttpStatusCode>(nameof(LmiOrchestrationTrigger.TransformItemActivity), A<Uri>.Ignored)).Returns(transformItemResult);

            // Act
            var result = await lmiOrchestrationTrigger.RefreshJobGroupOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PurgeSocActivity), A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<HttpStatusCode>(nameof(LmiOrchestrationTrigger.TransformItemActivity), A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PostTransformationEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        public async Task LmiOrchestrationTriggerRefreshJobGroupOrchestratorIsFailure(HttpStatusCode transformItemResult)
        {
            // Arrange
            const bool expectedResult = false;
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { SocId = Guid.NewGuid() });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<HttpStatusCode>(nameof(LmiOrchestrationTrigger.TransformItemActivity), A<Uri>.Ignored)).Returns(transformItemResult);

            // Act
            var result = await lmiOrchestrationTrigger.RefreshJobGroupOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PurgeSocActivity), A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<HttpStatusCode>(nameof(LmiOrchestrationTrigger.TransformItemActivity), A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PostTransformationEventActivity), A<EventGridPostRequestModel>.Ignored)).MustNotHaveHappened();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiOrchestrationTriggerPurgeJobGroupOrchestratorIsSuccessful()
        {
            // Arrange
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { SocId = Guid.NewGuid() });

            // Act
            await lmiOrchestrationTrigger.PurgeJobGroupOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PurgeSocActivity), A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PostTransformationEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiOrchestrationTriggerPurgeOrchestratorIsSuccessful()
        {
            // Arrange
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { SocId = Guid.NewGuid() });

            // Act
            await lmiOrchestrationTrigger.PurgeOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PurgeActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PostTransformationEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiOrchestrationTriggerRefreshOrchestratorIsSuccessful()
        {
            // Arrange
            const int summariesCount = 2;
            var dummySummaries = A.CollectionOfDummy<SummaryItem>(summariesCount);
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { SocId = Guid.NewGuid() });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SummaryItem>>(nameof(LmiOrchestrationTrigger.GetGraphSummaryItemsActivity), A<object>.Ignored)).Returns(dummySummaries);

            // Act
            await lmiOrchestrationTrigger.RefreshOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SummaryItem>>(nameof(LmiOrchestrationTrigger.GetGraphSummaryItemsActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PurgeActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<HttpStatusCode>(nameof(LmiOrchestrationTrigger.TransformItemActivity), A<Uri>.Ignored)).MustHaveHappened(summariesCount, Times.Exactly);
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PostTransformationEventActivity), A<EventGridPostRequestModel>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task LmiOrchestrationTriggerRefreshOrchestratorIsSuccessfulWhenNoData()
        {
            // Arrange
            const int summariesCount = 0;
            var dummySummaries = A.CollectionOfDummy<SummaryItem>(summariesCount);
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { SocId = Guid.NewGuid() });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SummaryItem>>(nameof(LmiOrchestrationTrigger.GetGraphSummaryItemsActivity), A<object>.Ignored)).Returns(dummySummaries);

            // Act
            await lmiOrchestrationTrigger.RefreshOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SummaryItem>>(nameof(LmiOrchestrationTrigger.GetGraphSummaryItemsActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PurgeActivity), A<object>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<HttpStatusCode>(nameof(LmiOrchestrationTrigger.TransformItemActivity), A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PostTransformationEventActivity), A<EventGridPostRequestModel>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task LmiOrchestrationTriggerRefreshOrchestratorIsSuccessfulWhenNullData()
        {
            // Arrange
            List<SummaryItem>? nullSummaries = null;
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).Returns(new SocRequestModel { SocId = Guid.NewGuid() });
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SummaryItem>>(nameof(LmiOrchestrationTrigger.GetGraphSummaryItemsActivity), A<object>.Ignored)).Returns(nullSummaries);

            // Act
            await lmiOrchestrationTrigger.RefreshOrchestrator(fakeDurableOrchestrationContext).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDurableOrchestrationContext.GetInput<SocRequestModel>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<IList<SummaryItem>>(nameof(LmiOrchestrationTrigger.GetGraphSummaryItemsActivity), A<object>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PurgeActivity), A<object>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync<HttpStatusCode>(nameof(LmiOrchestrationTrigger.TransformItemActivity), A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDurableOrchestrationContext.CallActivityAsync(nameof(LmiOrchestrationTrigger.PostTransformationEventActivity), A<EventGridPostRequestModel>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task LmiOrchestrationTriggerGetGraphSummaryItemsActivityIsSuccessful()
        {
            // Arrange
            const int summariesCount = 2;
            var dummySummaries = A.CollectionOfDummy<SummaryItem>(summariesCount);
            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).Returns(dummySummaries);

            // Act
            var results = await lmiOrchestrationTrigger.GetGraphSummaryItemsActivity(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).MustHaveHappenedOnceExactly();
            Assert.Equal(dummySummaries, results);
        }

        [Fact]
        public async Task LmiOrchestrationTriggerPurgeActivityIsSuccessful()
        {
            // Arrange
            const bool expectedResult = true;
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).Returns(expectedResult);

            // Act
            var result = await lmiOrchestrationTrigger.PurgeActivity(null).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupDocumentService.PurgeAsync()).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiOrchestrationTriggerPurgeSocActivityIsSuccessful()
        {
            // Arrange
            const bool expectedResult = true;
            A.CallTo(() => fakeJobGroupDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(expectedResult);

            // Act
            var result = await lmiOrchestrationTrigger.PurgeSocActivity(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeJobGroupDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiOrchestrationTriggerTransformItemAsyncIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var dummyLmiSoc = A.Dummy<LmiSoc>();
            var dummyJobgroup = A.Dummy<JobGroupModel>();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).Returns(dummyLmiSoc);
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).Returns(dummyJobgroup);
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(dummyJobgroup);
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).Returns(expectedResult);

            // Act
            var result = await lmiOrchestrationTrigger.TransformItemActivity(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiOrchestrationTriggerTransformItemAsyncReturnsBadRequestWhenNoItem()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            var dummyLmiSoc = A.Dummy<LmiSoc>();
            JobGroupModel? nullJobgroup = null;
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).Returns(dummyLmiSoc);
            A.CallTo(() => fakeMapper.Map<JobGroupModel?>(A<LmiSoc>.Ignored)).Returns(nullJobgroup);

            // Act
            var result = await lmiOrchestrationTrigger.TransformItemActivity(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<JobGroupModel?>(A<LmiSoc>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeJobGroupDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeJobGroupDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustNotHaveHappened();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task LmiOrchestrationTriggerPostGraphEventActivityIsSuccessful()
        {
            // Arrange
            var eventGridPostRequest = new EventGridPostRequestModel
            {
                SocId = Guid.NewGuid(),
                DisplayText = "Display text",
                EventType = "published",
            };

            // Act
            await lmiOrchestrationTrigger.PostTransformationEventActivity(eventGridPostRequest).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeEventGridService.SendEventAsync(A<EventGridEventData>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}
