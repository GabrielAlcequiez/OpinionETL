using Microsoft.EntityFrameworkCore;
using OpinionETL.Core;
using OpinionETL.Core.Interfaces;
using OpinionETL.Core.Models;
using OpinionETL.Data.Context;
using OpinionETL.Data.Extraction.Csv;
using OpinionETL.Data.Extraction.Csv.Mapping;
using OpinionETL.Data.Extraction.Csv.Models;

namespace OpinionETL.Data.Importers
{
    public class SourceTypeImporter : ISourceTypeImporter
    {
        private readonly AppDbContext _context;

        public SourceTypeImporter(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportResult> ImportAsync(CancellationToken ct = default)
        {
            var extractor = new CsvExtractor();
            string dataPath = Path.Combine(AppContext.BaseDirectory, "data");

            var result = new ImportResult()
            {
                TotalImported = 0,
                TotalDuplicated = 0,
                TotalRead = 0,
                TotalRejected = 0
            };

            var existingSourceTypes = (await _context.Source_Type
                    .Select(x => x.Type)
                    .ToListAsync(ct))
                    .Select(x => x.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var sourceTypes = extractor.Extract<DataSourceCsvRow, DataSourceCsvMap>(Path.Combine(dataPath, "fuente_datos.csv"));

            int pendingImport = 0;

            foreach (var sourceType in sourceTypes)
            {
                result.TotalRead++;
                
                if(string.IsNullOrWhiteSpace(sourceType.TipoFuente))
                {
                    result.TotalRejected++;
                    continue;
                }

                string type = sourceType.TipoFuente.Trim();

                if (!existingSourceTypes.Add(type))
                {
                    result.TotalDuplicated++;
                    continue;
                }

                var entity = new SourceType
                {
                    Type = type
                };

                _context.Source_Type.Add(entity);
                pendingImport++;
            }

            await _context.SaveChangesAsync(ct);
            result.TotalImported = pendingImport;
            return result;
        }
    }
}
