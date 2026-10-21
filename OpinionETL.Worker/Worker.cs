using OpinionETL.Core.Interfaces;

namespace OpinionETL.Worker;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var pipeline = scope.ServiceProvider
                .GetRequiredService<IEtlPipeline>();

            await pipeline.ExecuteAsync(stoppingToken);

            _logger.LogInformation(
                "Ejecución del Worker finalizada correctamente");
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Ejecución del Worker cancelada");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "El Worker terminó debido a un error");

            throw;
        }
    }
}
