using Microsoft.Extensions.Logging;
using OpenAI;
using Spt.Rag.Shared.Models;

namespace Spt.Rag.Shared.Embedding
{
    public class EmbeddingModelAzureOpenAI(
        OpenAIClient client,
        ILogger logger,
        string modelVersion,
        int expectedDimensions)
        : IEmbeddingModel
    {
        public string ModelVersion { get; } = modelVersion;

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            int attempt = 0;
            while (attempt < 3)
            {
                try
                {
                    var result = await client.GetEmbeddingsAsync(new EmbeddingsOptions(ModelVersion, new[] { text }));
                    var embedding = result.Value.Data[0].Embedding;
                    if (embedding.Count != expectedDimensions)
                    {
                        logger.LogWarning($"Embedding dimensions mismatch: expected {expectedDimensions}, got {embedding.Count}");
                        Array.Resize(ref embedding, expectedDimensions);
                    }
                    return embedding.ToArray();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Embedding generation failed. Attempt {Attempt}", attempt + 1);
                    await Task.Delay(1000 * (int)Math.Pow(2, attempt));
                    attempt++;
                }
            }

            throw new Exception("Failed to generate embedding after 3 retries.");
        }
    }
}