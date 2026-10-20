using CsvHelper.Configuration;
using OpinionETL.Data.Extraction.Csv.Models;

namespace OpinionETL.Data.Extraction.Csv.Mapping
{
    public class DataSourceCsvMap : ClassMap<DataSourceCsvRow>
    {
        public DataSourceCsvMap()
        {
            Map(x => x.IdFuente).Name("IdFuente");
            Map(x => x.TipoFuente).Name("TipoFuente");
            Map(x => x.FechaCarga).Name("FechaCarga");
        }

    }
}
