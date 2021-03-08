using AutoMapper;
using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Models;
using DFC.Api.Lmi.Transformation.Models.ContentApiModels;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Api.Lmi.Transformation.Services;
using DFC.Compui.Cosmos.Contracts;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
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
        private readonly IEventGridService fakeEventGridService = A.Fake<IEventGridService>();
        private readonly TransformationService transformationService;
        private readonly EventGridClientOptions eventGridClientOptions = new EventGridClientOptions
        {
            ApiEndpoint = new Uri("https://somewhere.com", UriKind.Absolute),
            SubjectPrefix = "SubjectPrefix",
            TopicEndpoint = "TopicEndpoint",
            TopicKey = "TopicKey",
        };

        public TransformationServiceTests()
        {
            transformationService = new TransformationService(fakeLogger, fakeMapper, fakeContentTypeMappingService, fakeCmsApiService, fakeDocumentService, fakeEventGridService, eventGridClientOptions);
        }

        [Fact]
        public async Task TransformationServiceTransformIsSuccessful()
        {
            // Arrange
            const int collectionCount = 2;
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var dummyModels = A.CollectionOfDummy<SummaryItem>(collectionCount);
            var dummyLmiSocModel = A.Dummy<LmiSoc>();
            var dummyJobGroupModel = A.Dummy<JobGroupModel>();

            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).Returns(dummyModels);
            A.CallTo(() => fakeDocumentService.PurgeAsync()).Returns(true);
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).Returns(dummyLmiSocModel);
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).Returns(dummyJobGroupModel);
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(A.Dummy<JobGroupModel>());
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).Returns(HttpStatusCode.OK);

            // Act
            var result = await transformationService.TransformAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.PurgeAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).MustHaveHappened(collectionCount, Times.Exactly);
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).MustHaveHappened(collectionCount, Times.Exactly);
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappened(collectionCount, Times.Exactly);
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustHaveHappened(collectionCount, Times.Exactly);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task TransformationServiceTransformReturnsNoSummaries()
        {
            // Arrange
            const int collectionCount = 0;
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var noModels = A.CollectionOfDummy<SummaryItem>(collectionCount);

            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).Returns(noModels);

            // Act
            var result = await transformationService.TransformAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.PurgeAsync()).MustNotHaveHappened();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task TransformationServiceTransformReturnsNullForSummaries()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            IList<SummaryItem>? nullModels = default;

            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).Returns(nullModels);

            // Act
            var result = await transformationService.TransformAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetSummaryAsync<SummaryItem>()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.PurgeAsync()).MustNotHaveHappened();
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task TransformationServiceTransformItemIsSuccessful()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.OK;
            var dummyLmiSocModel = A.Dummy<LmiSoc>();
            var dummyJobGroupModel = A.Dummy<JobGroupModel>();

            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).Returns(dummyLmiSocModel);
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).Returns(dummyJobGroupModel);
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(A.Dummy<JobGroupModel>());
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).Returns(HttpStatusCode.OK);

            // Act
            var result = await transformationService.TransformItemAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task TransformationServiceTransformItemForNoItemReturnsBadRequest()
        {
            // Arrange
            const HttpStatusCode expectedResult = HttpStatusCode.BadRequest;
            JobGroupModel? nullJobGroupModel = null;

            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).Returns(A.Dummy<LmiSoc>());
            A.CallTo(() => fakeMapper.Map<JobGroupModel?>(A<LmiSoc>.Ignored)).Returns(nullJobGroupModel);
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).Returns(A.Dummy<JobGroupModel>());
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).Returns(HttpStatusCode.OK);

            // Act
            var result = await transformationService.TransformItemAsync(new Uri("https://somewhere.com", UriKind.Absolute)).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeCmsApiService.GetItemAsync<LmiSoc>(A<Uri>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeMapper.Map<JobGroupModel>(A<LmiSoc>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeDocumentService.GetAsync(A<Expression<Func<JobGroupModel, bool>>>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => fakeDocumentService.UpsertAsync(A<JobGroupModel>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task TransformationServicePurgeIsSuccessful()
        {
            // Arrange
            const bool expectedResult = true;

            A.CallTo(() => fakeDocumentService.PurgeAsync()).Returns(true);

            // Act
            var result = await transformationService.PurgeAsync().ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.PurgeAsync()).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task TransformationServiceDeleteIsSuccessful()
        {
            // Arrange
            const bool expectedResult = true;

            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).Returns(true);

            // Act
            var result = await transformationService.DeleteAsync(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeDocumentService.DeleteAsync(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedResult, result);
        }
    }
}
