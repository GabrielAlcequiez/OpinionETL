namespace OpinionETL.Data.Extraction.Csv.Models
{
    public class DataSourceCsvRow
    {
        public string IdFuente { get; set; } = string.Empty;
        public string TipoFuente { get; set; } = string.Empty;
        public string FechaCarga { get; set; } = string.Empty;
    }
}
