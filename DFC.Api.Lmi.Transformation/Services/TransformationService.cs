using AutoMapper;
using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Enums;
using DFC.Api.Lmi.Transformation.Models;
using DFC.Api.Lmi.Transformation.Models.ContentApiModels;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Services
{
    public class TransformationService : ITransformationService
    {
        private readonly ILogger<TransformationService> logger;
        private readonly IMapper mapper;
        private readonly ICmsApiService cmsApiService;
        private readonly IDocumentService<JobGroupModel> jobGroupDocumentService;
        private readonly IEventGridService eventGridService;
        private readonly EventGridClientOptions eventGridClientOptions;

        public TransformationService(
            ILogger<TransformationService> logger,
            IMapper mapper,
            IContentTypeMappingService contentTypeMappingService,
            ICmsApiService cmsApiService,
            IDocumentService<JobGroupModel> jobGroupDocumentService,
            IEventGridService eventGridService,
            EventGridClientOptions eventGridClientOptions)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.cmsApiService = cmsApiService;
            this.jobGroupDocumentService = jobGroupDocumentService;
            this.eventGridService = eventGridService;
            this.eventGridClientOptions = eventGridClientOptions;

            contentTypeMappingService.AddMapping(nameof(LmiSocJobProfile), typeof(LmiSocJobProfile));
            contentTypeMappingService.AddMapping(nameof(LmiSocPredicted), typeof(LmiSocPredicted));
            contentTypeMappingService.AddMapping(nameof(LmiSocPredictedYear), typeof(LmiSocPredictedYear));
            contentTypeMappingService.AddMapping(nameof(LmiSocBreakdown), typeof(LmiSocBreakdown));
            contentTypeMappingService.AddMapping(nameof(LmiSocBreakdownYear), typeof(LmiSocBreakdownYear));
            contentTypeMappingService.AddMapping(nameof(LmiSocBreakdownYearValue), typeof(LmiSocBreakdownYearValue));
        }

        public async Task<HttpStatusCode> TransformAsync()
        {
            logger.LogInformation("Loading summary list from content API");
            var summaries = await cmsApiService.GetSummaryAsync<SummaryItem>().ConfigureAwait(false);

            if (summaries != null && summaries.Any())
            {
                await PurgeAsync().ConfigureAwait(false);

                foreach (var item in summaries.OrderBy(o => o.Soc))
                {
                    await TransformItemAsync(item.Url!).ConfigureAwait(false);
                }
            }

            var eventGridEventData = new EventGridEventData
            {
                ItemId = Guid.NewGuid().ToString(),
                Api = $"{eventGridClientOptions.ApiEndpoint}",
                DisplayText = "LMI transformed into job-groups",
                VersionId = Guid.NewGuid().ToString(),
                Author = eventGridClientOptions.SubjectPrefix,
            };

            await eventGridService.SendEventAsync(WebhookCacheOperation.CreateOrUpdate, eventGridEventData, eventGridClientOptions.SubjectPrefix).ConfigureAwait(false);

            logger.LogInformation($"Refreshed all Job Groups from Summary list");

            return HttpStatusCode.OK;
        }

        public async Task<HttpStatusCode> TransformItemAsync(Uri url)
        {
            logger.LogInformation($"Loading Job Group item: {url}");
            var lmiSoc = await cmsApiService.GetItemAsync<LmiSoc>(url!).ConfigureAwait(false);

            logger.LogInformation($"Transforming Job Group item for {url} to cache");
            var jobGroup = mapper.Map<JobGroupModel>(lmiSoc);

            if (jobGroup != null)
            {
                var existingJobGroup = await jobGroupDocumentService.GetAsync(w => w.Soc == jobGroup.Soc, jobGroup.Soc.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
                if (existingJobGroup != null)
                {
                    jobGroup.Etag = existingJobGroup.Etag;
                }

                logger.LogInformation($"Upserting Job Groups item: {jobGroup.Soc} / {url}");
                return await jobGroupDocumentService.UpsertAsync(jobGroup).ConfigureAwait(false);
            }

            return HttpStatusCode.BadRequest;
        }

        public async Task<bool> PurgeAsync()
        {
            logger.LogInformation("Purging all Job Groups");
            return await jobGroupDocumentService.PurgeAsync().ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync(Guid contentId)
        {
            logger.LogInformation($"Deleting Job Groups item: {contentId}");
            return await jobGroupDocumentService.DeleteAsync(contentId).ConfigureAwait(false);
        }
    }
}