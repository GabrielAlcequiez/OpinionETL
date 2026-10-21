namespace OpinionETL.Core.Interfaces
{
    public interface IOpinionProcessor
    {
        Task<ImportResult> ProcessAsync(CancellationToken ct = default);
    }
}
