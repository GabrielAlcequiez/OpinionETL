namespace OpinionETL.Core
{
    public class ImportResult
    {
        public int TotalRead { get; set; }
        public int TotalImported { get; set; }
        public int TotalDuplicated { get; set; }
        public int TotalRejected { get; set; }
    }
}
