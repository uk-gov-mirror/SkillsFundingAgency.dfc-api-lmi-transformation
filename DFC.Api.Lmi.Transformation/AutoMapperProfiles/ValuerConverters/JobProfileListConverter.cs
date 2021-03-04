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
    public class JobProfileListConverter : IValueConverter<IList<IBaseContentItemModel>?, List<JobProfileModel>?>
    {
        public List<JobProfileModel>? Convert(IList<IBaseContentItemModel>? sourceMember, ResolutionContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (sourceMember == null || !sourceMember.Any())
            {
                return default;
            }

            var results = new List<JobProfileModel>();

            foreach (var item in sourceMember)
            {
                switch (item.ContentType)
                {
                    case nameof(LmiSocJobProfile):
                        if (item is LmiSocJobProfile lmiSocJobProfile)
                        {
                            results.Add(context.Mapper.Map<JobProfileModel>(lmiSocJobProfile));
                        }

                        break;
                }
            }

            return results.OrderBy(o => o.Title).ToList();
        }
    }
}
