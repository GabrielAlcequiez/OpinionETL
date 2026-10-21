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
    public class ProductImporter : IProductImporter
    {
        private readonly AppDbContext _context;

        public ProductImporter(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportResult> ImportAsync(CancellationToken ct = default)
        {
            var extractor = new CsvExtractor();
            string dataPath = Path.Combine(AppContext.BaseDirectory, "data");
            var result = new ImportResult();

            var existingProduct = (await _context.Product
                .Select(x => x.ID)
                .ToHashSetAsync(ct));
            
            var categories = await _context.Category
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Name, x=> x.ID, StringComparer.OrdinalIgnoreCase, ct);

            var products = extractor.Extract<ProductCsvRow, ProductCsvMap>(Path.Combine(dataPath, "products.csv"));

            int pendingImport = 0;
            foreach (var product in products)
            {
                result.TotalRead++;

                if (String.IsNullOrWhiteSpace(product.Nombre) || string.IsNullOrWhiteSpace(product.IdProducto)){
                    result.TotalRejected++;
                    continue;
                }

                if(!int.TryParse(product.IdProducto, out int productId))
                {
                    result.TotalRejected++;
                    continue;
                }

                if (!existingProduct.Add(productId))
                {
                    result.TotalDuplicated++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(product.Categoria) ||
                   !categories.TryGetValue(product.Categoria.Trim(), out int categoryId))
                {
                    result.TotalRejected++;
                    continue;
                }

                var entity = new Product
                {
                    ID = productId,
                    Name = product.Nombre,
                    Category_ID = categoryId
                };

                _context.Product.Add(entity);
                pendingImport++;
            }

            await _context.SaveChangesAsync(ct);
            result.TotalImported = pendingImport;
            return result;


        }
    }
}
