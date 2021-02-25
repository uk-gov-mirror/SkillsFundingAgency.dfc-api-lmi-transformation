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
    public class BreakdownYearItemListConverter : IValueConverter<IList<IBaseContentItemModel>?, List<BreakdownYearItemModel>?>
    {
        public List<BreakdownYearItemModel>? Convert(IList<IBaseContentItemModel>? sourceMember, ResolutionContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (sourceMember == null || !sourceMember.Any())
            {
                return default;
            }

            var results = new List<BreakdownYearItemModel>();

            foreach (var item in sourceMember)
            {
                switch (item.ContentType)
                {
                    case nameof(LmiSocBreakdownYearItem):
                        if (item is LmiSocBreakdownYearItem lmiSocBreakdownYearItem)
                        {
                            var model = context.Mapper.Map<BreakdownYearItemModel>(lmiSocBreakdownYearItem);

                            switch (lmiSocBreakdownYearItem.Measure)
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
                    case Constants.MeasureForQualification:
                        results = results.OrderByDescending(o => o.Employment).Take(1).ToList();
                        break;
                    case Constants.MeasureForIndustry:
                        results = results.OrderByDescending(o => o.Employment).Take(10).ToList();
                        break;
                }
            }

            return results;
        }
    }
}
