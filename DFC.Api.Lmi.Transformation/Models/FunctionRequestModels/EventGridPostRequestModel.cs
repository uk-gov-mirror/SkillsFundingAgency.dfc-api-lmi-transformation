using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.FunctionRequestModels
{
    [ExcludeFromCodeCoverage]
    public class EventGridPostRequestModel
    {
        public Uri? Url { get; set; }

        public string? DisplayText { get; set; }

        public string? EventType { get; set; }
    }
}
