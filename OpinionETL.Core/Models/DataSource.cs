namespace OpinionETL.Core.Models
{
    public class DataSource
    {
        public int ID { get; set; }
        public int ExternalID { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Load_Date { get; set; }
        public int SourceTypeId { get; set; }
        public SourceType? SourceType { get; set; }
    }
}
