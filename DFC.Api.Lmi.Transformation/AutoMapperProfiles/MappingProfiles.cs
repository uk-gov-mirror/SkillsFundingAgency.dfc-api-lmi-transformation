using AutoMapper;
using DFC.Api.Lmi.Transformation.AutoMapperProfiles.ValuerConverters;
using DFC.Api.Lmi.Transformation.Models.ContentApiModels;
using DFC.Api.Lmi.Transformation.Models.JobGroupModels;
using DFC.Content.Pkg.Netcore.Data.Models;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.Lmi.Transformation.AutoMapperProfiles
{
    [ExcludeFromCodeCoverage]
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<JobGroupModel, JobGroupSummaryItemModel>();

            CreateMap<LmiSoc, JobGroupModel>()
                .ForMember(d => d.Id, s => s.MapFrom(m => m.ItemId))
                .ForMember(d => d.Title, s => s.MapFrom(m => m.Title))
                .ForMember(d => d.TransformedDate, s => s.Ignore())
                .ForMember(d => d.JobProfiles, opt => opt.ConvertUsing(new JobProfileListConverter(), a => a.ContentItems))
                .ForMember(d => d.JobGrowth, opt => opt.ConvertUsing(new JobGrowthConverter(), a => a.ContentItems))
                .ForMember(d => d.QualificationLevel, opt => opt.ConvertUsing(new QualificationLevelConverter(), a => a.ContentItems))
                .ForMember(d => d.EmploymentByRegion, opt => opt.ConvertUsing(new EmploymentByRegionConverter(), a => a.ContentItems))
                .ForMember(d => d.TopIndustriesInJobGroup, opt => opt.ConvertUsing(new TopIndustriesInJobGroupConverter(), a => a.ContentItems));

            CreateMap<LmiSocJobProfile, JobProfileModel>();

            CreateMap<LmiSocPredicted, PredictedModel>()
                .ForMember(d => d.PredictedEmployment, opt => opt.ConvertUsing(new PredictedYearListConverter(), a => a.ContentItems));

            CreateMap<LmiSocPredictedYear, PredictedYearModel>();

            CreateMap<LmiSocBreakdown, BreakdownModel>()
                .ForMember(d => d.PredictedEmployment, opt => opt.ConvertUsing(new BreakdownYearListConverter(), a => a.ContentItems));

            CreateMap<LmiSocBreakdownYear, BreakdownYearModel>()
                .ForMember(d => d.Breakdown, opt => opt.ConvertUsing(new BreakdownYearValuesConverter(), a => a.ContentItems));

            CreateMap<LmiSocBreakdownYearValue, BreakdownYearValueModel>();

            CreateMap<LinkDetails, LmiSocJobProfile>()
                .ForMember(d => d.Url, s => s.Ignore())
                .ForMember(d => d.ItemId, s => s.Ignore())
                .ForMember(d => d.Title, s => s.Ignore())
                .ForMember(d => d.Published, s => s.Ignore())
                .ForMember(d => d.CreatedDate, s => s.Ignore())
                .ForMember(d => d.Links, s => s.Ignore())
                .ForMember(d => d.ContentLinks, s => s.Ignore())
                .ForMember(d => d.ContentItems, s => s.Ignore());

            CreateMap<LinkDetails, LmiSocPredicted>()
                .ForMember(d => d.Url, s => s.Ignore())
                .ForMember(d => d.ItemId, s => s.Ignore())
                .ForMember(d => d.Title, s => s.Ignore())
                .ForMember(d => d.Published, s => s.Ignore())
                .ForMember(d => d.CreatedDate, s => s.Ignore())
                .ForMember(d => d.Links, s => s.Ignore())
                .ForMember(d => d.ContentLinks, s => s.Ignore())
                .ForMember(d => d.ContentItems, s => s.Ignore());

            CreateMap<LinkDetails, LmiSocPredictedYear>()
                .ForMember(d => d.Url, s => s.Ignore())
                .ForMember(d => d.ItemId, s => s.Ignore())
                .ForMember(d => d.Title, s => s.Ignore())
                .ForMember(d => d.Published, s => s.Ignore())
                .ForMember(d => d.CreatedDate, s => s.Ignore())
                .ForMember(d => d.Links, s => s.Ignore())
                .ForMember(d => d.ContentLinks, s => s.Ignore())
                .ForMember(d => d.ContentItems, s => s.Ignore());

            CreateMap<LinkDetails, LmiSocBreakdown>()
                .ForMember(d => d.Url, s => s.Ignore())
                .ForMember(d => d.ItemId, s => s.Ignore())
                .ForMember(d => d.Title, s => s.Ignore())
                .ForMember(d => d.Published, s => s.Ignore())
                .ForMember(d => d.CreatedDate, s => s.Ignore())
                .ForMember(d => d.Links, s => s.Ignore())
                .ForMember(d => d.ContentLinks, s => s.Ignore())
                .ForMember(d => d.ContentItems, s => s.Ignore());

            CreateMap<LinkDetails, LmiSocBreakdownYear>()
                .ForMember(d => d.Url, s => s.Ignore())
                .ForMember(d => d.ItemId, s => s.Ignore())
                .ForMember(d => d.Title, s => s.Ignore())
                .ForMember(d => d.Published, s => s.Ignore())
                .ForMember(d => d.CreatedDate, s => s.Ignore())
                .ForMember(d => d.Links, s => s.Ignore())
                .ForMember(d => d.ContentLinks, s => s.Ignore())
                .ForMember(d => d.ContentItems, s => s.Ignore());

            CreateMap<LinkDetails, LmiSocBreakdownYearValue>()
                .ForMember(d => d.Url, s => s.Ignore())
                .ForMember(d => d.ItemId, s => s.Ignore())
                .ForMember(d => d.Title, s => s.Ignore())
                .ForMember(d => d.Published, s => s.Ignore())
                .ForMember(d => d.CreatedDate, s => s.Ignore())
                .ForMember(d => d.Links, s => s.Ignore())
                .ForMember(d => d.ContentLinks, s => s.Ignore())
                .ForMember(d => d.ContentItems, s => s.Ignore());
        }
    }
}
