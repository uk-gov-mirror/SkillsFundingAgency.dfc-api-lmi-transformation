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
    public class BreakdownYearListConverter : IValueConverter<IList<IBaseContentItemModel>?, List<BreakdownYearModel>?>
    {
        public List<BreakdownYearModel>? Convert(IList<IBaseContentItemModel>? sourceMember, ResolutionContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (sourceMember == null || !sourceMember.Any())
            {
                return default;
            }

            var results = new List<BreakdownYearModel>();

            foreach (var item in sourceMember)
            {
                switch (item.ContentType)
                {
                    case nameof(LmiSocBreakdownYear):
                        if (item is LmiSocBreakdownYear lmiSocBreakdownYear)
                        {
                            results.Add(context.Mapper.Map<BreakdownYearModel>(lmiSocBreakdownYear));
                        }

                        break;
                }
            }

            var firstYearOnlyMeasures = new[] { Constants.MeasureForQualification, Constants.MeasureForIndustry, Constants.MeasureForRegion };

            if (results.Any() && results.First().Measure != null && firstYearOnlyMeasures.Contains(results.First().Measure))
            {
                results = results.OrderBy(o => o.Year).Take(1).ToList();
            }

            return results;
        }
    }
}
