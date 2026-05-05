using AsyncReportEngine.Shared.Dtos.Reports;

namespace AsyncReportEngine.Services.Abstraction;

public interface IBenchmarkService
{
    Task<BenchmarkRunDto> RunSyncBenchmarkAsync(BenchmarkRequestDto dto);
    Task<Guid> RunAsyncBenchmarkAsync(BenchmarkRequestDto dto, string userId);
    Task<BenchmarkRunDto?> GetRunAsync(Guid runId);
    Task<List<BenchmarkRunDto>> GetHistoryAsync(int take = 20);
}