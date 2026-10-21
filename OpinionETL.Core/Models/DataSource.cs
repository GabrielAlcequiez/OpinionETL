namespace OpinionETL.Core.Models
{
    public class DataSource
    {
        public int ID { get; set; }
        public string? External_ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Load_Date { get; set; }
        public int Source_Type_Id { get; set; }

    }
}
