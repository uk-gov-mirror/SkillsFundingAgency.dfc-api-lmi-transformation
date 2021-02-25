using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.JobGroupModels
{
    [ExcludeFromCodeCoverage]
    public class PredictedModel
    {
        public int Soc { get; set; }

        public string? Measure { get; set; }

        public IList<PredictedYearModel>? PredictedEmployment { get; set; }
    }
}
