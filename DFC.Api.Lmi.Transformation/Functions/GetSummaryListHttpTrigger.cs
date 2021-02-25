using AutoMapper;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Functions
{
    public class GetSummaryListHttpTrigger
    {
        private readonly ILogger<GetSummaryListHttpTrigger> logger;
        private readonly IMapper mapper;
        private readonly IDocumentService<JobGroupModel> documentService;

        public GetSummaryListHttpTrigger(
           ILogger<GetSummaryListHttpTrigger> logger,
           IMapper mapper,
           IDocumentService<JobGroupModel> documentService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.documentService = documentService;
        }

        [FunctionName("GetSummaryList")]
        [Display(Name = "Get summary list", Description = "Receives a summary list of job-groups.")]
        [ProducesResponseType(typeof(IList<JobGroupSummaryItemModel>), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Summary list retrieved", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "job-groups/")] HttpRequest? request)
        {
            logger.LogInformation("Getting all job-groups");

            var jobGroupModels = await documentService.GetAllAsync().ConfigureAwait(false);

            if (jobGroupModels != null && jobGroupModels.Any())
            {
                logger.LogInformation($"Returning {jobGroupModels.Count()} job-group summary items");

                var summaryModels = mapper.Map<IList<JobGroupSummaryItemModel>>(jobGroupModels);

                return new OkObjectResult(summaryModels.OrderBy(o => o.Soc));
            }

            logger.LogWarning("Failed to get any job-groups");

            return new NoContentResult();
        }
    }
}