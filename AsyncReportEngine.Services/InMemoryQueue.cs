using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos;
using System.Threading.Channels;

namespace AsyncReportEngine.Services;

public class InMemoryQueue : IInMemoryQueue
{
    private readonly Channel<ReportGenerationMessage> queue;

    public InMemoryQueue()
    {
        // Unbounded - черга без ліміту (обмежена лише оперативною пам'яттю сервера)
        var options = new UnboundedChannelOptions
        {
            SingleWriter = false, // Багато HTTP-запитів можуть писати одночасно
            SingleReader = true   // Тільки один BackgroundService буде читати
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
        // ReadAllAsync автоматично "засинає", якщо черга порожня, і прокидається, коли є дані
        return queue.Reader.ReadAllAsync(cancellationToken);
    }
}
