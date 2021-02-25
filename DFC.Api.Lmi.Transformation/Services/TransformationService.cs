using AutoMapper;
using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Models.ContentApiModels;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Services
{
    public class TransformationService : ITransformationService
    {
        private readonly ILogger<TransformationService> logger;
        private readonly IMapper mapper;
        private readonly ICmsApiService cmsApiService;
        private readonly IDocumentService<JobGroupModel> documentService;

        public TransformationService(
            ILogger<TransformationService> logger,
            IMapper mapper,
            IContentTypeMappingService contentTypeMappingService,
            ICmsApiService cmsApiService,
            IDocumentService<JobGroupModel> documentService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.cmsApiService = cmsApiService;
            this.documentService = documentService;

            contentTypeMappingService.AddMapping(nameof(LmiSocJobProfile), typeof(LmiSocJobProfile));
            contentTypeMappingService.AddMapping(nameof(LmiSocPredicted), typeof(LmiSocPredicted));
            contentTypeMappingService.AddMapping(nameof(LmiSocPredictedYear), typeof(LmiSocPredictedYear));
            contentTypeMappingService.AddMapping(nameof(LmiSocBreakdown), typeof(LmiSocBreakdown));
            contentTypeMappingService.AddMapping(nameof(LmiSocBreakdownYear), typeof(LmiSocBreakdownYear));
            contentTypeMappingService.AddMapping(nameof(LmiSocBreakdownYearValue), typeof(LmiSocBreakdownYearValue));
        }

        public async Task GetAndTransformAsync()
        {
            logger.LogInformation("Loading summary list from content API");
            var summaries = await cmsApiService.GetSummaryAsync<SummaryItem>().ConfigureAwait(false);

            if (summaries != null && summaries.Any())
            {
                logger.LogInformation("Purging Cosmos cache");
                await documentService.PurgeAsync().ConfigureAwait(false);
                logger.LogInformation("Purged Cosmos cache");

                foreach (var item in summaries.OrderBy(o => o.Soc))
                {
                    logger.LogInformation($"Loading item for {item.Url} from content API");
                    var lmiSoc = await cmsApiService.GetItemAsync<LmiSoc>(item.Url!).ConfigureAwait(false);

                    logger.LogInformation($"Transforming item for {item.Url} to cache");
                    var jobGroup = mapper.Map<JobGroupModel>(lmiSoc);

                    logger.LogInformation($"Saving item for {item.Url} to cache");
                    await documentService.UpsertAsync(jobGroup).ConfigureAwait(false);
                    logger.LogInformation($"Saved item for {item.Url} to cache");
                }
            }
        }
    }
}