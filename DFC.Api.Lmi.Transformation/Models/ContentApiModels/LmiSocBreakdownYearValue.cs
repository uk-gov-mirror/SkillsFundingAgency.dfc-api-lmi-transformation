using DFC.Content.Pkg.Netcore.Data.Models;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.ContentApiModels
{
    [ExcludeFromCodeCoverage]
    public class LmiSocBreakdownYearValue : BaseContentItemModel
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
