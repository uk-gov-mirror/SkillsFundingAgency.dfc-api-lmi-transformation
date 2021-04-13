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
    public class PredictedYearListConverter : IValueConverter<IList<IBaseContentItemModel>?, List<PredictedYearModel>?>
    {
        public List<PredictedYearModel>? Convert(IList<IBaseContentItemModel>? sourceMember, ResolutionContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (sourceMember == null || !sourceMember.Any())
            {
                return default;
            }

            var results = new List<PredictedYearModel>();

            foreach (var item in sourceMember)
            {
                switch (item.ContentType)
                {
                    case nameof(LmiSocPredictedYear):
                        if (item is LmiSocPredictedYear lmiSocPredictedYear)
                        {
                            results.Add(context.Mapper.Map<PredictedYearModel>(lmiSocPredictedYear));
                        }

                        break;
                }
            }

            return results;
        }
    }
}
