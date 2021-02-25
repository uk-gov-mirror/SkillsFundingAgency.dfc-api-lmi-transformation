using DFC.Content.Pkg.Netcore.Data.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models.ContentApiModels
{
    [ExcludeFromCodeCoverage]
    public class LmiSoc : BaseContentItemModel
    {
        [JsonProperty(PropertyName = "skos__prefLabel")]
        public int Soc { get; set; }

        [JsonProperty("Title")]
        public new string? Title { get; set; }

        public string? Description { get; set; }

        public string? Qualifications { get; set; }

        public string? Tasks { get; set; }
    }
}
