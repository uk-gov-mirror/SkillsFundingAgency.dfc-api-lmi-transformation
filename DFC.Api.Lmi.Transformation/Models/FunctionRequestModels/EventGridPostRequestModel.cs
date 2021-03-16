using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.FunctionRequestModels
{
    [ExcludeFromCodeCoverage]
    public class EventGridPostRequestModel
    {
        public Guid? SocId { get; set; }

        public string? Api { get; set; }

        public string? DisplayText { get; set; }

        public string? EventType { get; set; }
    }
}
