using AsyncReportEngine.Shared.Dtos.Reports;
using AsyncReportEngine.Shared.Entities;
using AutoMapper;

namespace AsyncReportEngine.Shared.MappingProfiles;

public class ReportProfile : Profile
{
    public ReportProfile()
    {
        CreateMap<ReportRequest, ReportHistoryDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
