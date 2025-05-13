namespace Spt.Rag.Shared.Models;

public interface IEmbeddingModel
{
    string ModelVersion { get; }
    Task<float[]> GenerateEmbeddingAsync(string text);
}