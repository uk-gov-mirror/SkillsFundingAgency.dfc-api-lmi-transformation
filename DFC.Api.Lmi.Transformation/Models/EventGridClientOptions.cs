using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models
{
    [ExcludeFromCodeCoverage]
    public class EventGridClientOptions
    {
        public string? TopicEndpoint { get; set; }

        public string? SubjectPrefix { get; set; }

        public string? TopicKey { get; set; }

        public Uri? ApiEndpoint { get; set; }
    }
}
