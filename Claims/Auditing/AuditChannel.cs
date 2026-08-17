using System.Threading.Channels;

namespace Claims.Auditing;

public record AuditMessage(string EntityType, string EntityId, string Action);

/// <summary>
/// In-memory channel for asynchronous audit message queuing.
/// </summary>
public class AuditChannel
{
    private readonly Channel<AuditMessage> _channel;

    public AuditChannel()
    {
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        
        _channel = Channel.CreateBounded<AuditMessage>(options);
    }

    public async ValueTask AddAuditAsync(AuditMessage message)
    {
        await _channel.Writer.WriteAsync(message);
    }

    public IAsyncEnumerable<AuditMessage> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}