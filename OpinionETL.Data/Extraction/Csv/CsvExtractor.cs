using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace OpinionETL.Data.Extraction.Csv
{
public class CsvExtractor
    {
        public List<T> Extract<T, TMap>(string path)
            where T : class 
            where TMap : ClassMap<T>
        {
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<TMap>();

            return csv.GetRecords<T>().ToList();
        }
    }
}
