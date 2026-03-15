using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Reports;
using AutoMapper;

namespace AsyncReportEngine.Services;

public class ReportHistoryService : IReportHistoryService
{
    private readonly IReportRepository reportRepository;
    private readonly IMapper mapper;

    public ReportHistoryService(IReportRepository reportRepository, IMapper mapper)
    {
        this.reportRepository = reportRepository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<ReportHistoryDto>> GetUserHistoryAsync(string userId, int take)
    {
        var history = await reportRepository.GetUserRequestsHistoryAsync(userId, take);

        return mapper.Map<IEnumerable<ReportHistoryDto>>(history);
    }
}
