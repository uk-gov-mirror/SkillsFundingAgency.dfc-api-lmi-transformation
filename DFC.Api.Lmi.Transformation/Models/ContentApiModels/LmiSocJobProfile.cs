using DFC.Content.Pkg.Netcore.Data.Models;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.ContentApiModels
{
    [ExcludeFromCodeCoverage]
    public class LmiSocJobProfile : BaseContentItemModel
    {
        [JsonProperty("skos__prefLabel")]
        public string? CanonicalName { get; set; }

        [JsonProperty("Title")]
        public new string? Title { get; set; }
    }
}
