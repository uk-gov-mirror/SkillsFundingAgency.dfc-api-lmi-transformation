using AutoMapper;
using DFC.Api.Lmi.Transformation.Models.ContentApiModels;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Api.Lmi.Transformation.Services;
using DFC.Compui.Cosmos.Contracts;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Lmi.Transformation.UnitTests.Services
{
    [Trait("Category", "TransformationService - service Unit Tests")]
    public class TransformationServiceTests
    {
        private readonly ILogger<TransformationService> fakeLogger = A.Fake<ILogger<TransformationService>>();
        private readonly IMapper fakeMapper = A.Fake<IMapper>();
        private readonly IContentTypeMappingService fakeContentTypeMappingService = A.Fake<IContentTypeMappingService>();
        private readonly ICmsApiService fakeCmsApiService = A.Fake<ICmsApiService>();
        private readonly IDocumentService<JobGroupModel> fakeDocumentService = A.Fake<IDocumentService<JobGroupModel>>();
        private readonly TransformationService transformationService;

        public TransformationServiceTests()
        {
            transformationService = new TransformationService(fakeLogger, fakeMapper, fakeContentTypeMappingService, fakeCmsApiService, fakeDocumentService);
        }

        [Fact]
        public async Task TransformationServiceGetAndTransformIsSuccessful()
        {
            // Arrange
            const int collectionCount = 2;
            var dummyModels = A.CollectionOfDummy<SummaryItem>(collectionCount);
            var dummyLmiSocModel = A.Dummy<LmiSoc>();
            var dummyJobGroupModel = A.Dummy<JobGroupModel>();

            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).Returns(dummyModels);
            A.CallTo(() => fakeDocumentService.PurgeAsync()).Returns(true);
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).Returns(dummyLmiSocModel);
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).Returns(dummyJobGroupModel);

            // Act
            await transformationService.GetAndTransformAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.PurgeAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).MustHaveHappened(collectionCount, Times.Exactly);
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).MustHaveHappened(collectionCount, Times.Exactly);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustHaveHappened(collectionCount, Times.Exactly);

            Assert.True(true);
        }

        [Fact]
        public async Task TransformationServiceGetAndTransformReturnsNoSummaries()
        {
            // Arrange
            const int collectionCount = 0;
            var noModels = A.CollectionOfDummy<SummaryItem>(collectionCount);

            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).Returns(noModels);

            // Act
            await transformationService.GetAndTransformAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.PurgeAsync()).MustNotHaveHappened();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustNotHaveHappened();

            Assert.True(true);
        }

        [Fact]
        public async Task TransformationServiceGetAndTransformReturnsNullForSummaries()
        {
            // Arrange
            IList<SummaryItem>? nullModels = default;

            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).Returns(nullModels);

            // Act
            await transformationService.GetAndTransformAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.PurgeAsync()).MustNotHaveHappened();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustNotHaveHappened();

            Assert.True(true);
        }
    }
}
