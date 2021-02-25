using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.JobGroupModels
{
    [ExcludeFromCodeCoverage]
    public class PredictedYearModel
    {
        public int Soc { get; set; }

        public string? Measure { get; set; }

        public int Year { get; set; }

        public decimal Employment { get; set; }
    }
}
