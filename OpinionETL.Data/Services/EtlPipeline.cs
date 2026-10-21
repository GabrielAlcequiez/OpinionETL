using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpinionETL.Core.Interfaces;

namespace OpinionETL.Core.Services;

public class EtlPipeline : IEtlPipeline
{
    private readonly ICategoryImporter _categories;
    private readonly ISourceTypeImporter _sourceTypes;
    private readonly IClassificationImporter _classifications;
    private readonly IClientImporter _clients;
    private readonly IProductImporter _products;
    private readonly IDataSourceImporter _dataSources;
    private readonly IOpinionStagingImporter _staging;
    private readonly IOpinionProcessor _processor;
    private readonly ILogger<EtlPipeline> _logger;

    public EtlPipeline(
        ICategoryImporter categories,
        ISourceTypeImporter sourceTypes,
        IClassificationImporter classifications,
        IClientImporter clients,
        IProductImporter products,
        IDataSourceImporter dataSources,
        IOpinionStagingImporter staging,
        IOpinionProcessor processor,
        ILogger<EtlPipeline> logger)
    {
        _categories = categories;
        _sourceTypes = sourceTypes;
        _classifications = classifications;
        _clients = clients;
        _products = products;
        _dataSources = dataSources;
        _staging = staging;
        _processor = processor;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var totalTimer = Stopwatch.StartNew();

        _logger.LogInformation("Iniciando pipeline ETL");

        // Todo está en orden, cargando las entidades maestra primero, luego las fuentes de datos y finalmente procesando las opiniones.
        try
        {
            // 1. Catálogos
            await RunStageAsync(
                "Categorías", _categories.ImportAsync, ct);

            await RunStageAsync(
                "Tipos de fuente", _sourceTypes.ImportAsync, ct);

            await RunStageAsync(
                "Clasificaciones", _classifications.ImportAsync, ct);

            // 2. Datos maestros
            await RunStageAsync(
                "Clientes", _clients.ImportAsync, ct);

            await RunStageAsync(
                "Productos", _products.ImportAsync, ct);

            // 3. Fuentes
            await RunStageAsync(
                "Fuentes de datos", _dataSources.ImportAsync, ct);

            // 4. Staging
            await RunStageAsync(
                "Opiniones en Staging", _staging.ImportAsync, ct);

            // 5. Transformación y carga final
            await RunStageAsync(
                "Procesamiento de opiniones",
                _processor.ProcessAsync,
                ct);

            _logger.LogInformation(
                "Pipeline ETL completado correctamente en {ElapsedMs} ms",
                totalTimer.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("Pipeline ETL cancelado");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error durante la ejecución del pipeline ETL");

            throw;
        }
        finally
        {
            totalTimer.Stop();
        }
    }

    private async Task RunStageAsync(
        string stageName,
        Func<CancellationToken, Task<ImportResult>> execute,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Iniciando etapa: {Stage}",
            stageName);

        try
        {
            var result = await execute(ct);

            _logger.LogInformation(
                "Etapa {Stage} completada: " +
                "Leídos={Read}, Importados={Imported}, " +
                "Duplicados={Duplicated}, Rechazados={Rejected}, " +
                "Duración={ElapsedMs} ms",
                stageName,
                result.TotalRead,
                result.TotalImported,
                result.TotalDuplicated,
                result.TotalRejected,
                stopwatch.ElapsedMilliseconds);

            if (result.TotalRejected > 0)
            {
                _logger.LogWarning(
                    "La etapa {Stage} rechazó {Rejected} registros",
                    stageName,
                    result.TotalRejected);
            }
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}
