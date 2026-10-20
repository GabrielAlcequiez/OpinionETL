namespace OpinionETL.Core.Models
{
    public class StageOpinion
    {
        public int ID { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string OriginalOpinionId { get; set; } = string.Empty;
        public string? OriginalClientId { get; set; } 
        public string? OriginalProductId { get; set; }

        public string? Comment { get; set; }
        public string? Original_Date { get; set; }
        public string? Original_Classification { get; set; }
        public string? Original_Score { get; set; }
        public string? Source_Name{ get; set; }
        public string Status { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public DateTime LoadedAt { get; set; }

    }
}
