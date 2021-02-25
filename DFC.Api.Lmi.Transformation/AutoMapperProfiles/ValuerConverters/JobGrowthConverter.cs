using AutoMapper;
using DFC.Api.Lmi.Transformation.Models.ContentApiModels;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Content.Pkg.Netcore.Data.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DFC.Api.Lmi.Transformation.AutoMapperProfiles.ValuerConverters
{
    [ExcludeFromCodeCoverage]
    public class JobGrowthConverter : IValueConverter<IList<IBaseContentItemModel>?, JobGrowthPredictionModel?>
    {
        public JobGrowthPredictionModel? Convert(IList<IBaseContentItemModel>? sourceMember, ResolutionContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (sourceMember == null || !sourceMember.Any())
            {
                return default;
            }

            var results = new List<PredictedModel>();

            foreach (var item in sourceMember)
            {
                switch (item.ContentType)
                {
                    case nameof(LmiSocPredicted):
                        if (item is LmiSocPredicted lmiSocPredicted)
                        {
                            results.Add(context.Mapper.Map<PredictedModel>(lmiSocPredicted));
                        }

                        break;
                }
            }

            var predictedEmployment = results.FirstOrDefault()?.PredictedEmployment;
            if (predictedEmployment != null)
            {
                var firstYearResult = predictedEmployment.OrderBy(o => o.Year).FirstOrDefault();
                var lastYearResult = predictedEmployment.OrderByDescending(o => o.Year).FirstOrDefault();

                if (firstYearResult != null && lastYearResult != null)
                {
                    var result = new JobGrowthPredictionModel()
                    {
                        StartYearRange = firstYearResult.Year,
                        EndYearRange = lastYearResult.Year,
                        JobsCreated = lastYearResult.Employment - firstYearResult.Employment,
                        PercentageGrowth = (lastYearResult.Employment - firstYearResult.Employment) / firstYearResult.Employment * 100,
                    };

                    return result;
                }
            }

            return default;
        }
    }
}
