using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.JobGroupModels
{
    [ExcludeFromCodeCoverage]
    public class BreakdownYearItemModel
    {
        public int Soc { get; set; }

        public string? Measure { get; set; }

        public int Year { get; set; }

        public int Code { get; set; }

        public string? Note { get; set; }

        public string? Name { get; set; }

        public decimal Employment { get; set; }
    }
}
