using DFC.Api.Lmi.Transformation.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Models
{
    [ExcludeFromCodeCoverage]
    public class EnvironmentValues
    {
        public string EnvironmentNameApiSuffix { get; set; } = Environment.GetEnvironmentVariable(Constants.EnvironmentNameApiSuffix) ?? string.Empty;

        public bool IsDraftEnvironment => !string.IsNullOrWhiteSpace(EnvironmentNameApiSuffix);
    }
}
