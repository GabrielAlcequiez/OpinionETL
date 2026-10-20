using OpinionETL.Core;
using OpinionETL.Core.Interfaces;
using OpinionETL.Data.Context;
using OpinionETL.Data.Extraction.Csv;
using OpinionETL.Data.Extraction.Csv.Mapping;
using OpinionETL.Data.Extraction.Csv.Models;
using Microsoft.EntityFrameworkCore;
using OpinionETL.Core.Models;

namespace OpinionETL.Data.Importers
{
    public class CategoryImporter : ICategoryImporter
    {
        private readonly AppDbContext _context;

        public CategoryImporter(AppDbContext context)
        {
            _context = context;
        }

        // Acá realmente iteraremos en el CSV de productos, porque acá estan las categorias, la idea es extrerlas
        // y guardarlas en la tabla de categorias, para luego poder relacionarlas con los productos

        // Y ya en el importer de product, con el mismo csv se hace una comparación de las categorias que vienen
        // en el csv con las que ya existen en la base de datos
        public async Task<ImportResult> ImportAsync(CancellationToken ct = default)
        {
            var extractor = new CsvExtractor();
            string dataPath = Path.Combine(AppContext.BaseDirectory, "data");

            var existingCategories = (await _context.Category
                .Select(x => x.Name)
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

            var products = extractor.Extract<ProductCsvRow, ProductCsvMap>(Path.Combine(dataPath, "products.csv"));

            int pendingImport = 0;
            foreach (var product in products)
            {
                result.TotalRead++;

                if (string.IsNullOrWhiteSpace(product.Categoria))
                {
                    result.TotalRejected++;
                    continue;
                };

                if(!existingCategories.Add(product.Categoria.Trim()))
                {
                    result.TotalDuplicated++;
                    continue;
                }

                var entity = new Category()
                {
                    Name = product.Categoria
                };

                _context.Category.Add(entity);
                pendingImport++;
            }
            await _context.SaveChangesAsync();
            result.TotalImported = pendingImport;
            return result;
        }
    }
}
