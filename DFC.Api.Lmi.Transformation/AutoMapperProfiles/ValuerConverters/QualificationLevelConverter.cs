using AutoMapper;
using DFC.Api.Lmi.Transformation.Common;
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
    public class QualificationLevelConverter : IValueConverter<IList<IBaseContentItemModel>?, QualificationLevelModel?>
    {
        public QualificationLevelModel? Convert(IList<IBaseContentItemModel>? sourceMember, ResolutionContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (sourceMember == null || !sourceMember.Any())
            {
                return default;
            }

            var results = new List<BreakdownModel>();

            foreach (var item in sourceMember)
            {
                switch (item.ContentType)
                {
                    case nameof(LmiSocBreakdown):
                        if (item is LmiSocBreakdown lmiSocBreakdown && lmiSocBreakdown.Measure != null && lmiSocBreakdown.Measure.Equals(Constants.MeasureForQualification))
                        {
                            results.Add(context.Mapper.Map<BreakdownModel>(lmiSocBreakdown));
                        }

                        break;
                }
            }

            var predictedEmployment = results.FirstOrDefault()?.PredictedEmployment;
            if (predictedEmployment != null)
            {
                var firstYearResult = predictedEmployment.OrderBy(o => o.Year).FirstOrDefault();

                if (firstYearResult != null)
                {
                    var maxEmploymentBreakdown = firstYearResult.Breakdown.OrderByDescending(o => o.Employment).First();

                    if (maxEmploymentBreakdown != null)
                    {
                        var result = new QualificationLevelModel()
                        {
                            Year = firstYearResult.Year,
                            Code = maxEmploymentBreakdown.Code,
                            Name = maxEmploymentBreakdown.Name,
                            Note = maxEmploymentBreakdown.Note,
                            Employment = maxEmploymentBreakdown.Employment,
                        };

                        return result;
                    }
                }
            }

            return default;
        }
    }
}
