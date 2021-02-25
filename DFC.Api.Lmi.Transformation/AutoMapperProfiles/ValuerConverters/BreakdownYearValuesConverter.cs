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
    public class BreakdownYearValuesConverter : IValueConverter<IList<IBaseContentItemModel>?, List<BreakdownYearValueModel>?>
    {
        public List<BreakdownYearValueModel>? Convert(IList<IBaseContentItemModel>? sourceMember, ResolutionContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (sourceMember == null || !sourceMember.Any())
            {
                return default;
            }

            var results = new List<BreakdownYearValueModel>();

            foreach (var item in sourceMember)
            {
                switch (item.ContentType)
                {
                    case nameof(LmiSocBreakdownYearValue):
                        if (item is LmiSocBreakdownYearValue lmiSocBreakdownYearValues)
                        {
                            var model = context.Mapper.Map<BreakdownYearValueModel>(lmiSocBreakdownYearValues);

                            switch (lmiSocBreakdownYearValues.Measure)
                            {
                                case Constants.MeasureForRegion:
                                    var excludeRegions = new[] { Constants.RegionCodeForWales, Constants.RegionCodeForScotland, Constants.RegionCodeForNorthernIreland };

                                    if (!excludeRegions.Contains(model.Code))
                                    {
                                        results.Add(model);
                                    }

                                    break;
                                default:
                                    results.Add(model);
                                    break;
                            }
                        }

                        break;
                }
            }

            if (results.Any() && results.First().Measure != null)
            {
                switch (results.First().Measure)
                {
                    case Constants.MeasureForIndustry:
                        results = results.OrderByDescending(o => o.Employment).Take(10).ToList();
                        break;
                }
            }

            return results;
        }
    }
}
