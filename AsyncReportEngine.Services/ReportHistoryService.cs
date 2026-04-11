using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Pagination;
using AsyncReportEngine.Shared.Dtos.Reports;
using AsyncReportEngine.Shared.Enum;
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

    public async Task<PagedResult<ReportHistoryDto>> GetHistoryPagedAsync(int page, int pageSize, string? status)
    {
        ReportStatus? parsedStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReportStatus>(status, true, out var s))
        {
            parsedStatus = s;
        }

        int skip = (page - 1) * pageSize;
        var (items, totalCount) = await reportRepository.GetRequestsPagedAsync(skip, pageSize, parsedStatus);

        var dtos = items.Select(r => new ReportHistoryDto
        {
            Id = r.Id,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt,
            FinishedAt = r.FinishedAt,
            FileUrl = r.FileUrl,
            ErrorMessage = r.ErrorMessage
        }).ToList();

        return new PagedResult<ReportHistoryDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<ReportStatsDto> GetStatsAsync()
    {
        var rawStats = await reportRepository.GetRequestsStatsAsync();
        var result = new ReportStatsDto();

        foreach (var kvp in rawStats)
        {
            result.TotalReports += kvp.Value;

            if (kvp.Key == ReportStatus.Pending || kvp.Key == ReportStatus.Processing)
                result.PendingReports += kvp.Value;
            else if (kvp.Key == ReportStatus.Completed)
                result.CompletedReports += kvp.Value;
            else if (kvp.Key == ReportStatus.Failed)
                result.FailedReports += kvp.Value;
        }

        return result;
    }
}
