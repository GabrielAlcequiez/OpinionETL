using CsvHelper.Configuration;
using OpinionETL.Data.Extraction.Csv.Models;

namespace OpinionETL.Data.Extraction.Csv.Mapping
{
    public sealed class SocialCommentCsvMap : ClassMap<SocialCommentCsvRow>
    {
        public SocialCommentCsvMap()
        {
            Map(x => x.IdComment).Name("IdComment");
            Map(x => x.IdCliente).Name("IdCliente");
            Map(x => x.IdProducto).Name("IdProducto");
            Map(x => x.Fuente).Name("Fuente");
            Map(x => x.Fecha).Name("Fecha");
            Map(x => x.Comentario).Name("Comentario");
        }
    }
}
