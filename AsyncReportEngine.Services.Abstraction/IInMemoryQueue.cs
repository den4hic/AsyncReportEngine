using AsyncReportEngine.Shared.Dtos;

namespace AsyncReportEngine.Services.Abstraction;

public interface IInMemoryQueue
{
    ValueTask EnqueueAsync(ReportGenerationMessage message);

    IAsyncEnumerable<ReportGenerationMessage> DequeueAsync(CancellationToken cancellationToken);
}
