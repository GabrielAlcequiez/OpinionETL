using Microsoft.EntityFrameworkCore;
using OpinionETL.Core;
using OpinionETL.Core.Interfaces;
using OpinionETL.Core.Models;
using OpinionETL.Data.Context;
using OpinionETL.Data.Helpers;

namespace OpinionETL.Data.Services;

public class OpinionProcessor : IOpinionProcessor
{
    private readonly AppDbContext _context;

    public OpinionProcessor(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ImportResult> ProcessAsync(
        CancellationToken ct = default)
    {
        var result = new ImportResult();

        // División por regiones

        #region Preparación de datos (especie de arrange)

        // opiniones pendientes
        var pendingOpinions = await _context.Stage_Opinion
            .Where(x => x.Status == "Pending")
            .ToListAsync(ct);

        if(pendingOpinions.Count == 0) // si no hay na' pues no consultar
            return result;

        // identificadores válidos
        var clientIds = (await _context.Client
            .Select(x => x.ID)
            .ToListAsync(ct))
            .ToHashSet();

        var productIds = (await _context.Product
            .Select(x => x.ID)
            .ToListAsync(ct))
            .ToHashSet();

        // clasificaciones
        var classifications = await _context.Classification
            .ToDictionaryAsync(
                x => x.Type,
                x => x.ID,
                StringComparer.OrdinalIgnoreCase,
                ct);

        // tipos de fuente
        var sourceTypes = await _context.Source_Type
            .ToDictionaryAsync(
                x => x.Type,
                x => x.ID,
                StringComparer.OrdinalIgnoreCase,
                ct);

        //  fuentes existentes
        var dataSources = await _context.Data_Source
            .ToListAsync(ct);

        // para evitar procesar duplicados
        var processedIds = (await _context.Opinion
            .Select(x => x.Stage_Opinion_ID)
            .ToListAsync(ct))
            .ToHashSet();

        int pendingImport = 0;

        #endregion

        await using var transaction =
            await _context.Database.BeginTransactionAsync(ct);

        foreach (var staging in pendingOpinions)
        {
            result.TotalRead++;

            if (processedIds.Contains(staging.ID))
            {
                staging.Status = "Processed";
                result.TotalDuplicated++;
                continue;
            }

            var date = DateNormalizer.Normalize(
                staging.Original_Date);

            if (date is null ||
                string.IsNullOrWhiteSpace(staging.Comment))
            {
                Reject(staging, "Fecha o comentario inválido");
                result.TotalRejected++;
                continue;
            }

            // Normalizar identificadores
            int? clientId = IdNormalizer.NormalizeId(
                staging.Original_Client_ID);

            int? productId = IdNormalizer.NormalizeId(
                staging.Original_Product_ID);

            // Verificar existencia en datos maestros
            if (clientId.HasValue &&
                !clientIds.Contains(clientId.Value))
            {
                clientId = null;
            }

            if (productId.HasValue &&
                !productIds.Contains(productId.Value))
            {
                productId = null;
            }

            // Normalizar puntuación
            int? score = ScoreNormalizer.Normalize(
                staging.Original_Score);

            // Determinar clasificación
            string classificationName =
                OpinionClassifier.Classify(
                    staging.Original_Classification,
                    score);

            if (!classifications.TryGetValue(
                    classificationName,
                    out int classificationId))
            {
                Reject(staging, "Clasificación no encontrada");
                result.TotalRejected++;
                continue;
            }

            // Resolver fuente
            var dataSource = ResolveDataSource(
                staging,
                sourceTypes,
                dataSources);

            if (dataSource is null)
            {
                Reject(staging, "Fuente de datos no encontrada");
                result.TotalRejected++;
                continue;
            }

            // Crear opinión definitiva
            var opinion = new Opinion
            {
                Comment = staging.Comment.Trim(),
                Date = date.Value,
                Satisfaction_Score = score,
                Client_ID = clientId,
                Product_ID = productId,
                Classification_ID = classificationId,
                Stage_Opinion_ID = staging.ID
            };

            
            if (dataSource.ID > 0)
            {
                opinion.Data_Source_ID = dataSource.ID;
            }
            else
            {
                // Obtener el ID identity de la nueva fuente
                // dentro de la misma transacción.
                await _context.SaveChangesAsync(ct);
                opinion.Data_Source_ID = dataSource.ID;
            }

            _context.Opinion.Add(opinion);

            staging.Status = "Processed";
            staging.Error_Message = null;

            processedIds.Add(staging.ID);
            pendingImport++;
        }

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        result.TotalImported = pendingImport;

        return result;
    }

    private DataSource? ResolveDataSource(
        StageOpinion staging,
        Dictionary<string, int> sourceTypes,
        List<DataSource> dataSources)
    {
        string type;
        string name;

        switch (staging.Origin)
        {
            case "Survey":
                type = "CSV";
                name = "Encuesta Interna";
                break;

            case "WebReview":
                type = "Web";
                name = "Reseña Web";
                break;

            case "SocialComment":
                type = "Red Social";
                name = staging.Source_Name?.Trim() ?? "";
                break;

            default:
                return null;
        }

        if (!sourceTypes.TryGetValue(type, out int typeId) ||
            string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var source = dataSources.FirstOrDefault(x =>
            x.Source_Type_Id == typeId &&
            string.Equals(
                x.Name,
                name,
                StringComparison.OrdinalIgnoreCase));

        if (source is not null)
            return source;

        // Las fuentes sociales específicas pueden crearse
        // sin identificador externo.
        if (staging.Origin != "SocialComment")
            return null;

        source = new DataSource
        {
            Name = name,
            Source_Type_Id = typeId,
            External_ID = null,
            Load_Date = DateTime.UtcNow
        };

        _context.Data_Source.Add(source);
        dataSources.Add(source);

        return source;
    }

    private static void Reject(
        StageOpinion staging,
        string reason)
    {
        staging.Status = "Rejected";
        staging.Error_Message = reason;
    }
}
