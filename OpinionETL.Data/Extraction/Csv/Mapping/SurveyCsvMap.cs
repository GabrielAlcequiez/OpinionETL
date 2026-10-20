using CsvHelper.Configuration;
using OpinionETL.Data.Extraction.Csv.Models;

namespace OpinionETL.Data.Extraction.Csv.Mapping
{
    public sealed class SurveyCsvMap : ClassMap<SurveyCsvRow>
    {
        public SurveyCsvMap()
        {
            Map(x => x.IdOpinion).Name("IdOpinion");
            Map(x => x.IdCliente).Name("IdCliente");
            Map(x => x.IdProducto).Name("IdProducto");
            Map(x => x.Fecha).Name("Fecha");
            Map(x => x.Comentario).Name("Comentario");
            Map(x => x.Clasificación).Name("Clasificación");
            Map(x => x.PuntajeSatisfacción).Name("PuntajeSatisfacción");
            Map(x => x.Fuente).Name("Fuente");
        }
    }
}
