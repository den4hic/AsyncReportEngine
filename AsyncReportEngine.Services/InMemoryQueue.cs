using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos;
using System.Threading.Channels;

namespace AsyncReportEngine.Services;

public class InMemoryQueue : IInMemoryQueue
{
    private readonly Channel<ReportGenerationMessage> queue;

    public InMemoryQueue()
    {
        var options = new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true
        };

        queue = Channel.CreateUnbounded<ReportGenerationMessage>(options);
    }

    public async ValueTask EnqueueAsync(ReportGenerationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        await queue.Writer.WriteAsync(message);
    }

    public IAsyncEnumerable<ReportGenerationMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        return queue.Reader.ReadAllAsync(cancellationToken);
    }
}
