using CsvHelper.Configuration;
using OpinionETL.Data.Extraction.Csv.Models;

namespace OpinionETL.Data.Extraction.Csv.Mapping
{
    public sealed class ClientCsvMap : ClassMap<ClientCsvRow>
    {
        public ClientCsvMap()
        {
            Map(x => x.IdCliente).Name("IdCliente");
            Map(x => x.Nombre).Name("Nombre");
            Map(x => x.Email).Name("Email");
        }
    }
}
