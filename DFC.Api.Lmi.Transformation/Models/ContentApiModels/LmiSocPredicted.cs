using DFC.Content.Pkg.Netcore.Data.Models;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.ContentApiModels
{
    [ExcludeFromCodeCoverage]
    public class LmiSocPredicted : BaseContentItemModel
    {
        public int Soc { get; set; }

        public string? Measure { get; set; }
    }
}
