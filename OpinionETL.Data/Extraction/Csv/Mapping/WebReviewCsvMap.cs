using CsvHelper.Configuration;
using OpinionETL.Data.Extraction.Csv.Models;

namespace OpinionETL.Data.Extraction.Csv.Mapping
{
    public sealed class WebReviewCsvMap : ClassMap<WebReviewCsvRow>
    {
        public WebReviewCsvMap()
        {
            Map(x => x.IdReview).Name("IdReview");
            Map(x => x.IdCliente).Name("IdCliente");
            Map(x => x.IdProducto).Name("IdProducto");
            Map(x => x.Fecha).Name("Fecha");
            Map(x => x.Comentario).Name("Comentario");
            Map(x => x.Rating).Name("Rating");
        }
    }
}
