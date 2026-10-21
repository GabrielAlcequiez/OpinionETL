namespace OpinionETL.Core.Models
{
    public class Opinion
    {
        public int ID { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int? Satisfaction_Score { get; set; }
        public DateTime Date { get; set; }
        public int? Client_ID{ get; set; }
        public int? Product_ID { get; set; }
        public int Data_Source_ID { get; set; }
        public int Classification_ID { get; set; }
        public int Stage_Opinion_ID { get; set; }

    }
}
