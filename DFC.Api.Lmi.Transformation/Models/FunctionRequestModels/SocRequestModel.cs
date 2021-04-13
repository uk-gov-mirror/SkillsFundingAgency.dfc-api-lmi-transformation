using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.FunctionRequestModels
{
    [ExcludeFromCodeCoverage]
    public class SocRequestModel : OrchestratorRequestModel
    {
        public Guid? SocId { get; set; }

        public Uri? Url { get; set; }
    }
}
