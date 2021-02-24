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
    public class QualificationLevelConverter : IValueConverter<IList<IBaseContentItemModel>?, List<BreakdownModel>?>
    {
        public List<BreakdownModel>? Convert(IList<IBaseContentItemModel>? sourceMember, ResolutionContext context)
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

            return results;
        }
    }
}
