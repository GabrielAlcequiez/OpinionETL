using Microsoft.EntityFrameworkCore;
using OpinionETL.Core;
using OpinionETL.Core.Interfaces;
using OpinionETL.Core.Models;
using OpinionETL.Data.Context;
using OpinionETL.Data.Extraction.Csv;
using OpinionETL.Data.Extraction.Csv.Mapping;
using OpinionETL.Data.Extraction.Csv.Models;
using System.Globalization;

namespace OpinionETL.Data.Importers
{
    public class DataSourceImporter : IDataSourceImporter
    {
        private readonly AppDbContext _context;

        public DataSourceImporter(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportResult> ImportAsync(
    CancellationToken ct = default)
        {
            var extractor = new CsvExtractor();

            string dataPath = Path.Combine(
                AppContext.BaseDirectory,
                "data",
                "fuente_datos.csv");

            var result = new ImportResult();

            var sourceNames = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["CSV"] = "Encuesta Interna",
                ["Web"] = "Reseña Web",
                ["Red Social"] = "No Identificada"
            };

            var sourceTypes = await _context.Source_Type
                .ToDictionaryAsync(
                    x => x.Type,
                    x => x.ID,
                    StringComparer.OrdinalIgnoreCase,
                    ct);

            var existingExternalIds = (await _context.Data_Source
                .Where(x => x.External_ID != null)
                .Select(x => x.External_ID!)
                .ToListAsync(ct))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var sources = extractor
                .Extract<DataSourceCsvRow, DataSourceCsvMap>(dataPath);

            int pendingImport = 0;

            foreach (var source in sources)
            {
                result.TotalRead++;

                if (string.IsNullOrWhiteSpace(source.IdFuente) ||
                    string.IsNullOrWhiteSpace(source.TipoFuente))
                {
                    result.TotalRejected++;
                    continue;
                }

                string externalId = source.IdFuente.Trim();
                string sourceType = source.TipoFuente.Trim();

                if (!DateTime.TryParseExact(
                        source.FechaCarga,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime loadDate))
                {
                    result.TotalRejected++;
                    continue;
                }

                
                if (!sourceTypes.TryGetValue(
                        sourceType,
                        out int sourceTypeId))
                {
                    result.TotalRejected++;
                    continue;
                }

                if (!existingExternalIds.Add(externalId))
                {
                    result.TotalDuplicated++;
                    continue;
                }

                if (!sourceNames.TryGetValue(sourceType, out string? name))
                {
                    result.TotalRejected++;
                    continue;
                }

                var dataSource = new DataSource
                {
                    External_ID = externalId,
                    Name = name,
                    Load_Date = loadDate,
                    Source_Type_Id = sourceTypeId
                };

                _context.Data_Source.Add(dataSource);
                pendingImport++;
            }

            await _context.SaveChangesAsync(ct);
            result.TotalImported = pendingImport;
            return result;
        }
    }
}
