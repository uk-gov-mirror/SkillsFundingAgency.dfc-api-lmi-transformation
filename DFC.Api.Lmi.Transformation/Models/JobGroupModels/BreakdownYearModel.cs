using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.JobGroupModels
{
    [ExcludeFromCodeCoverage]
    public class BreakdownYearModel
    {
        public int Soc { get; set; }

        public string? Measure { get; set; }

        public int Year { get; set; }

        public IList<BreakdownYearItemModel>? Breakdown { get; set; }
    }
}
