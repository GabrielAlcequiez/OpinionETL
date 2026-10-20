using CsvHelper.Configuration;
using OpinionETL.Data.Extraction.Csv.Models;

namespace OpinionETL.Data.Extraction.Csv.Mapping
{
    public class ProductCsvMap : ClassMap<ProductCsvRow>
    {
        public ProductCsvMap()
        {
            Map(x => x.IdProducto).Name("IdProducto");
            Map(x => x.Nombre).Name("Nombre");
            Map(x => x.Categoria).Name("Categoria");
        }
    }
}
