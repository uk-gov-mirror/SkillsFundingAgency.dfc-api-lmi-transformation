using AutoMapper;
using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Models;
using DFC.Api.Lmi.Transformation.Models.ContentApiModels;
using DFC.Api.Lmi.Transformation.Models.FunctionRequestModels;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Functions
{
    public class LmiOrchestrationTrigger
    {
        private const string EventTypeForDraft = "draft";
        private const string EventTypeForPublished = "published";
        private const string EventTypeForDraftDiscarded = "draft-discarded";
        private const string EventTypeForDeleted = "deleted";

        private readonly ILogger<LmiOrchestrationTrigger> logger;
        private readonly IMapper mapper;
        private readonly ICmsApiService cmsApiService;
        private readonly IDocumentService<JobGroupModel> jobGroupDocumentService;
        private readonly IEventGridService eventGridService;
        private readonly EventGridClientOptions eventGridClientOptions;

        public LmiOrchestrationTrigger(
            ILogger<LmiOrchestrationTrigger> logger,
            IMapper mapper,
            IContentTypeMappingService contentTypeMappingService,
            ICmsApiService cmsApiService,
            IDocumentService<JobGroupModel> jobGroupDocumentService,
            IEventGridService eventGridService,
            EventGridClientOptions eventGridClientOptions)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.jobGroupDocumentService = jobGroupDocumentService;
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

            //TODO: ian: need to initialize the telemetry properly
            Activity? activity = null;
            if (Activity.Current == null)
            {
                activity = new Activity(nameof(LmiWebhookHttpTrigger)).Start();
                activity.SetParentId(Guid.NewGuid().ToString());
            }
        }

        [FunctionName(nameof(RefreshJobGroupOrchestrator))]
        public async Task<bool> RefreshJobGroupOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var socRequest = context.GetInput<SocRequestModel>();

            await context.CallActivityAsync(nameof(PurgeSocActivity), socRequest.SocId).ConfigureAwait(true);

            var upsertResult = await context.CallActivityAsync<HttpStatusCode>(nameof(TransformItemActivity), socRequest.Url).ConfigureAwait(true);

            if (upsertResult == HttpStatusCode.OK || upsertResult == HttpStatusCode.Created)
            {
                var eventGridPostRequest = new EventGridPostRequestModel
                {
                    SocId = socRequest.SocId,
                    Api = $"{eventGridClientOptions.ApiEndpoint}/{socRequest.SocId}",
                    DisplayText = $"LMI transformed into job-group from {socRequest.Url}",
                    EventType = socRequest.IsDraftEnvironment ? EventTypeForDraft : EventTypeForPublished,
                };

                await context.CallActivityAsync(nameof(PostTransformationEventActivity), eventGridPostRequest).ConfigureAwait(true);

                return true;
            }

            return false;
        }

        [FunctionName(nameof(PurgeJobGroupOrchestrator))]
        public async Task PurgeJobGroupOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var socRequest = context.GetInput<SocRequestModel>();

            await context.CallActivityAsync(nameof(PurgeSocActivity), socRequest.SocId).ConfigureAwait(true);

            var eventGridPostRequest = new EventGridPostRequestModel
            {
                SocId = socRequest.SocId,
                Api = $"{eventGridClientOptions.ApiEndpoint}/{socRequest.SocId}",
                DisplayText = $"LMI purged job-group for {socRequest.SocId}",
                EventType = socRequest.IsDraftEnvironment ? EventTypeForDraftDiscarded : EventTypeForDeleted,
            };

            await context.CallActivityAsync(nameof(PostTransformationEventActivity), eventGridPostRequest).ConfigureAwait(true);
        }

        [FunctionName(nameof(PurgeOrchestrator))]
        public async Task PurgeOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var socRequest = context.GetInput<SocRequestModel>();

            await context.CallActivityAsync(nameof(PurgeActivity), null).ConfigureAwait(true);

            var eventGridPostRequest = new EventGridPostRequestModel
            {
                SocId = socRequest.SocId,
                Api = $"{eventGridClientOptions.ApiEndpoint}",
                DisplayText = "LMI purged all job-group ",
                EventType = socRequest.IsDraftEnvironment ? EventTypeForDraftDiscarded : EventTypeForDeleted,
            };

            await context.CallActivityAsync(nameof(PostTransformationEventActivity), eventGridPostRequest).ConfigureAwait(true);
        }

        [FunctionName(nameof(RefreshOrchestrator))]
        [Timeout("04:00:00")]
        public async Task RefreshOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var socRequest = context.GetInput<SocRequestModel>();
            var summaries = await context.CallActivityAsync<IList<SummaryItem>?>(nameof(GetGraphSummaryItemsActivity), null).ConfigureAwait(true);

            if (summaries != null && summaries.Any())
            {
                await context.CallActivityAsync(nameof(PurgeActivity), null).ConfigureAwait(true);

                logger.LogInformation($"transforming {summaries.Count} LMI Graph items");

                var parallelTasks = new List<Task<HttpStatusCode>>();

                foreach (var summaryItem in summaries.OrderBy(o => o.Soc))
                {
                    parallelTasks.Add(context.CallActivityAsync<HttpStatusCode>(nameof(TransformItemActivity), summaryItem.Url));
                }

                await Task.WhenAll(parallelTasks).ConfigureAwait(true);

                var eventGridPostRequest = new EventGridPostRequestModel
                {
                    SocId = socRequest.SocId,
                    Api = $"{eventGridClientOptions.ApiEndpoint}",
                    DisplayText = $"LMI transformed all job-groups from {socRequest.Url}",
                    EventType = socRequest.IsDraftEnvironment ? EventTypeForDraft : EventTypeForPublished,
                };

                await context.CallActivityAsync(nameof(PostTransformationEventActivity), eventGridPostRequest).ConfigureAwait(true);

                int transformedToJobGroupCount = parallelTasks.Count(t => t.Result == HttpStatusCode.OK || t.Result == HttpStatusCode.Created);

                logger.LogInformation($"Transformed to Job-group {transformedToJobGroupCount} of {summaries.Count} Graph SOCs");
            }
            else
            {
                logger.LogWarning("No data available LMI Graph - no data transformed");
            }
        }

        [FunctionName(nameof(GetGraphSummaryItemsActivity))]
        public async Task<IList<SummaryItem>?> GetGraphSummaryItemsActivity([ActivityTrigger] string? name)
        {
            logger.LogInformation("Getting LMI Graph summaries");

            return await cmsApiService.GetSummaryAsync<SummaryItem>().ConfigureAwait(false);
        }

        [FunctionName(nameof(PurgeActivity))]
        public async Task<bool> PurgeActivity([ActivityTrigger] string? name)
        {
            logger.LogInformation("Deleting all Job Groups");

            return await jobGroupDocumentService.PurgeAsync().ConfigureAwait(false);
        }

        [FunctionName(nameof(PurgeSocActivity))]
        public async Task<bool> PurgeSocActivity([ActivityTrigger] Guid socId)
        {
            logger.LogInformation($"Deleting Job Groups item: {socId}");

            return await jobGroupDocumentService.DeleteAsync(socId).ConfigureAwait(false);
        }

        [FunctionName(nameof(TransformItemActivity))]
        public async Task<HttpStatusCode> TransformItemActivity([ActivityTrigger] Uri? url)
        {
            _ = url ?? throw new ArgumentNullException(nameof(url));

            logger.LogInformation($"Loading Job Group item: {url}");
            var lmiSoc = await cmsApiService.GetItemAsync<LmiSoc>(url).ConfigureAwait(false);

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

        [FunctionName(nameof(PostTransformationEventActivity))]
        public async Task PostTransformationEventActivity([ActivityTrigger] EventGridPostRequestModel? eventGridPostRequest)
        {
            _ = eventGridPostRequest ?? throw new ArgumentNullException(nameof(eventGridPostRequest));

            logger.LogInformation($"Posting to event grid for: {eventGridPostRequest.DisplayText}: {eventGridPostRequest.EventType}");

            var eventGridEventData = new EventGridEventData
            {
                ItemId = $"{eventGridPostRequest.SocId}",
                Api = eventGridPostRequest.Api,
                DisplayText = eventGridPostRequest.DisplayText,
                VersionId = Guid.NewGuid().ToString(),
                Author = eventGridClientOptions.SubjectPrefix,
            };

            await eventGridService.SendEventAsync(eventGridEventData, eventGridClientOptions.SubjectPrefix, eventGridPostRequest.EventType).ConfigureAwait(false);
        }
    }
}
