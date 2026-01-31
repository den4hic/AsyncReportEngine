using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos;
using Azure.Storage.Queues;
using System.Text;
using System.Text.Json;

namespace AsyncReportEngine.Services;

public class QueueService : IQueueService
{
    private readonly QueueClient queueClient;

    public QueueService(QueueClient queueClient)
    {
        this.queueClient = queueClient;
    }
    public async Task SendMessageAsync(ReportGenerationMessage message)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(jsonMessage);
        var base64Message = Convert.ToBase64String(bytes);

        await queueClient.SendMessageAsync(base64Message);
    }
}
