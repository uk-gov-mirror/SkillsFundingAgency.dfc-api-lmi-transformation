using DFC.Compui.Cosmos.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DFC.Api.Lmi.Transformation.Models.JobGroupModels
{
    [ExcludeFromCodeCoverage]
    public class JobGroupModel : DocumentModel
    {
        public override string? PartitionKey
        {
            get => Soc.ToString(CultureInfo.InvariantCulture);

            set => Soc = int.Parse(value ?? "0", CultureInfo.InvariantCulture);
        }

        public int Soc { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime TransformedDate { get; set; } = DateTime.UtcNow;

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Qualifications { get; set; }

        public string? Tasks { get; set; }

        public IList<JobProfileModel>? JobProfiles { get; set; }

        public JobGrowthPredictionModel? JobGrowth { get; set; }

        public QualificationLevelModel? QualificationLevel { get; set; }

        public IList<BreakdownModel>? EmploymentByRegion { get; set; }

        public IList<BreakdownModel>? TopIndustriesInJobGroup { get; set; }
    }
}
