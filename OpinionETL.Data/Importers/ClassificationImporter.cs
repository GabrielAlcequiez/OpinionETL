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
    public class ClassificationImporter : IClassificationImporter
    {
        private readonly AppDbContext _context;

        public ClassificationImporter(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportResult> ImportAsync(CancellationToken ct = default)
        {
            var extractor = new CsvExtractor();
            string dataPath = Path.Combine(AppContext.BaseDirectory, "data");

            var existingClassifications = (await _context.Classification
                  .Select(x => x.Type)
                  .ToListAsync(ct))
                  .Select(x => x.Trim())
                  .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = new ImportResult()
            {
                TotalImported = 0,
                TotalDuplicated = 0,
                TotalRead = 0,
                TotalRejected = 0
            };

            // los saco de Survey porque acá estan ya definidos los 3 tipos, luego se podría implementar que se pudieran agregar
            // más tipos de clasificación desde un CSV, pero por ahora no es necesario
            var surveys = extractor.Extract<SurveyCsvRow, SurveyCsvMap>(Path.Combine(dataPath, "surveys_part1.csv"));

            int pendingImport = 0;
            foreach(var survey in surveys)
            {
                result.TotalRead++;

                if(string.IsNullOrWhiteSpace(survey.Clasificación))
                {
                    result.TotalRejected++;
                    continue;
                }

                string classificationType = survey.Clasificación.Trim();
                if (!existingClassifications.Add(classificationType))
                    {
                    result.TotalDuplicated++;
                    continue;
                }

                var entity = new Classification
                {
                    Type = classificationType
                };

                _context.Classification.Add(entity);
                pendingImport++;
            }
            await _context.SaveChangesAsync(ct);
            result.TotalImported = pendingImport;
            return result;
        }
    }
}
