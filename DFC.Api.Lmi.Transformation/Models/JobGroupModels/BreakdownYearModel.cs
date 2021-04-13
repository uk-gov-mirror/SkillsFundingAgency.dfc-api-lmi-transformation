using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.JobGroupModels
{
    [ExcludeFromCodeCoverage]
    public class BreakdownYearModel
    {
        [JsonIgnore]
        public string? Measure { get; set; }

        public int Year { get; set; }

        public IList<BreakdownYearValueModel>? Breakdown { get; set; }
    }
}
