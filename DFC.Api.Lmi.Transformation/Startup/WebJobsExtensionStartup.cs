using AutoMapper;
using AzureFunctions.Extensions.Swashbuckle;
using DFC.Api.Lmi.Transformation.Contracts;
using DFC.Api.Lmi.Transformation.Models;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Api.Lmi.Transformation.Services;
using DFC.Api.Lmi.Transformation.Startup;
using DFC.Compui.Cosmos;
using DFC.Compui.Cosmos.Contracts;
using DFC.Compui.Subscriptions.Pkg.Netstandard.Extensions;
using DFC.Content.Pkg.Netcore.Extensions;
using DFC.Swagger.Standard;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

[assembly: WebJobsStartup(typeof(WebJobsExtensionStartup), "Web Jobs Extension Startup")]

namespace DFC.Api.Lmi.Transformation.Startup
{
    [ExcludeFromCodeCoverage]
    public class WebJobsExtensionStartup : IWebJobsStartup
    {
        private const string CosmosDbLmiTransformationConfigAppSettings = "Configuration:CosmosDbConnections:LmiTransformation";

        public void Configure(IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var cosmosDbConnection = configuration.GetSection(CosmosDbLmiTransformationConfigAppSettings).Get<CosmosDbConnection>();

            builder.Services.AddSingleton(configuration.GetSection(nameof(EventGridClientOptions)).Get<EventGridClientOptions>() ?? new EventGridClientOptions());

            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            builder.Services.AddHttpClient();
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddAutoMapper(typeof(WebJobsExtensionStartup).Assembly);
            builder.Services.AddDocumentServices<JobGroupModel>(cosmosDbConnection, false);
            builder.Services.AddSubscriptionService(configuration);
            builder.Services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddTransient<ILmiWebhookReceiverService, LmiWebhookReceiverService>();
            builder.Services.AddTransient<IEventGridService, EventGridService>();
            builder.Services.AddTransient<IEventGridClientService, EventGridClientService>();

            var policyRegistry = builder.Services.AddPolicyRegistry();

            builder.Services.AddApiServices(configuration, policyRegistry);
        }
    }
}