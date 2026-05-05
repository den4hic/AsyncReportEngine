using AsyncReportEngine.Shared.Entities;

namespace AsyncReportEngine.DataAccess.Abstraction.Repositories;

public interface IBenchmarkRepository
{
    Task<Guid> CreateRunAsync(string approach, int requestedCount);
    Task<BenchmarkRun?> GetRunAsync(Guid runId);
    Task<List<BenchmarkRun>> GetHistoryAsync(int take = 20);
    Task TryCompleteRunAsync(Guid runId);
}
