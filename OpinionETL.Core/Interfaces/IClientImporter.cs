namespace OpinionETL.Core.Interfaces
{
    public interface IClientImporter
    {
        Task<ImportResult> ImportAsync(CancellationToken ct = default);
    }
}
