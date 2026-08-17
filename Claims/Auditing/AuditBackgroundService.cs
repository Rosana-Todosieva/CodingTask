using Microsoft.Extensions.Hosting;

namespace Claims.Auditing;

/// <summary>
/// Background service that continuously processes audit messages from the channel without blocking HTTP requests.
/// </summary>
public class AuditBackgroundService : BackgroundService
{
    private readonly AuditChannel _auditChannel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditBackgroundService> _logger;

    public AuditBackgroundService(
        AuditChannel auditChannel, 
        IServiceProvider serviceProvider, 
        ILogger<AuditBackgroundService> logger)
    {
        _auditChannel = auditChannel;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _auditChannel.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var auditer = scope.ServiceProvider.GetRequiredService<Auditer>();

                if (message.EntityType == "Claim")
                {
                    auditer.AuditClaim(message.EntityId, message.Action);
                }
                else if (message.EntityType == "Cover")
                {
                    auditer.AuditCover(message.EntityId, message.Action);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log for {EntityId}.", message.EntityId);
            }
        }
    }
}