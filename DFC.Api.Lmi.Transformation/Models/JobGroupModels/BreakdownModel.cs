using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.JobGroupModels
{
    [ExcludeFromCodeCoverage]
    public class BreakdownModel
    {
        public string? Note { get; set; }

        public IList<BreakdownYearModel>? PredictedEmployment { get; set; }
    }
}
