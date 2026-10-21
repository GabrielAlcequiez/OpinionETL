namespace OpinionETL.Core.Interfaces;

public interface IEtlPipeline
{
    Task ExecuteAsync(CancellationToken ct = default);
}