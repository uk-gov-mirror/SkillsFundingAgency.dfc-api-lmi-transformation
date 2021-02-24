using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.Common
{
    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        public const string MeasureForIndustry = "industry";
        public const string MeasureForQualification = "qualification";
        public const string MeasureForRegion = "region";

        public const int RegionCodeForWales = 10;
        public const int RegionCodeForScotland = 11;
        public const int RegionCodeForNorthernIreland = 12;
    }
}
