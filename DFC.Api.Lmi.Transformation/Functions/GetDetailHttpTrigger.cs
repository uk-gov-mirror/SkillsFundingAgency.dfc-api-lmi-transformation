using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Compui.Cosmos.Contracts;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Api.Lmi.Transformation.Functions
{
    public class GetDetailHttpTrigger
    {
        private readonly ILogger<GetDetailHttpTrigger> logger;
        private readonly IDocumentService<JobGroupModel> documentService;

        public GetDetailHttpTrigger(
           ILogger<GetDetailHttpTrigger> logger,
           IDocumentService<JobGroupModel> documentService)
        {
            this.logger = logger;
            this.documentService = documentService;
        }

        [FunctionName("GetDetail")]
        [Display(Name = "Get detail by SOC", Description = "Retrieves a job-group detail.")]
        [ProducesResponseType(typeof(JobGroupModel), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Detail retrieved", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Invalid request data", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.InternalServerError, Description = "Internal error caught and logged", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.TooManyRequests, Description = "Too many requests being sent, by default the API supports 150 per minute.", ShowSchema = false)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "job-groups/{socId}")] HttpRequest? request, Guid socId)
        {
            logger.LogInformation($"Getting job-group for {socId}");

            var jobGroupModel = await documentService.GetByIdAsync(socId).ConfigureAwait(false);

            if (jobGroupModel != null)
            {
                logger.LogInformation($"Returning {jobGroupModel.Soc} job-group detail");

                return new OkObjectResult(jobGroupModel);
            }

            logger.LogWarning($"Failed to get job-group for {socId}");

            return new NoContentResult();
        }
    }
}