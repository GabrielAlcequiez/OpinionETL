namespace OpinionETL.Data.Extraction.Csv.Models
{
    public class ProductCsvRow
    {
        public  string IdProducto { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
    }
}
