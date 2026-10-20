using OpinionETL.Core.Interfaces;
using OpinionETL.Data.Extraction.Csv;
using OpinionETL.Data.Extraction.Csv.Mapping;
using OpinionETL.Data.Extraction.Csv.Models;

namespace OpinionETL.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _lifetime;

        public Worker(
            IServiceScopeFactory scopeFactory,
            ILogger<Worker> logger,
            IHostApplicationLifetime lifetime)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _lifetime = lifetime;
        }
        protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var importer = scope.ServiceProvider
                    .GetRequiredService<ICategoryImporter>();

                var result = await importer.ImportAsync(stoppingToken);

                _logger.LogInformation(
                    "Leídos: {Read}, Importados: {Imported}, Duplicados: {Duplicated}, Rechazados: {Rejected}",
                    result.TotalRead,
                    result.TotalImported,
                    result.TotalDuplicated,
                    result.TotalRejected);
            }
            catch (Exception ex) when (
                ex is not OperationCanceledException ||
                !stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error al importar clientes");
            }
            finally
            {
                _lifetime.StopApplication();
            }
        }
    }
}
