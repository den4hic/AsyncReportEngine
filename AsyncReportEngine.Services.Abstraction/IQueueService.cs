using AsyncReportEngine.Shared.Dtos;

namespace AsyncReportEngine.Services.Abstraction;

public interface IQueueService
{
    Task SendMessageAsync(ReportGenerationMessage message);
}
