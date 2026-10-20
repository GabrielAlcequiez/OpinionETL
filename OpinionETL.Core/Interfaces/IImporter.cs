namespace OpinionETL.Core.Interfaces
{
    public interface IImporter
    {
        Task<ImportResult> ImportAsync(CancellationToken ct = default);
    }
}
