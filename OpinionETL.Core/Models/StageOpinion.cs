namespace OpinionETL.Core.Models
{
    public class StageOpinion
    {
        public int ID { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Original_Opinion_ID { get; set; } = string.Empty;
        public string? Original_Client_ID { get; set; } 
        public string? Original_Product_ID { get; set; }

        public string? Comment { get; set; }
        public string? Original_Date { get; set; }
        public string? Original_Classification { get; set; }
        public string? Original_Score { get; set; }
        public string? Source_Name{ get; set; }
        public string Status { get; set; } = string.Empty;

        public string? Error_Message { get; set; }
        public DateTime Loaded_At { get; set; }

    }
}
