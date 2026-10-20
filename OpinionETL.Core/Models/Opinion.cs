namespace OpinionETL.Core.Models
{
    public class Opinion
    {
        public int ID { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int? SatisfactionScore { get; set; }
        public DateTime Date { get; set; }
        public int? ClientId{ get; set; }
        public int? ProductId { get; set; }
        public int DataSourceId { get; set; }
        public int ClassificationId { get; set; }
        public int StageOpinionId { get; set; }

    }
}
