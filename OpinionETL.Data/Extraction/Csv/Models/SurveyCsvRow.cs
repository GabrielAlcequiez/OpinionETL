namespace OpinionETL.Data.Extraction.Csv.Models
{
    public class SurveyCsvRow
    {
        public string IdOpinion { get; set; } = string.Empty;
        public string IdCliente { get; set; } = string.Empty;
        public string IdProducto { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
        public string Clasificación { get; set; } = string.Empty;
        public string PuntajeSatisfacción { get; set; } = string.Empty;
        public string Fuente { get; set; } = string.Empty;
    }
}
