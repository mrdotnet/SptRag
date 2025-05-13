using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Spt.Rag.Shared.Storage
{
    public class EmbeddingStorage
    {
        private readonly SqlConnection _sql;
        private readonly ILogger _logger;

        public EmbeddingStorage(SqlConnection sql, ILogger logger)
        {
            _sql = sql;
            _logger = logger;
        }

        public async Task StoreEmbeddingAsync(string hash, string modelVersion, float[] embedding)
        {
            var jsonEmbedding = JsonSerializer.Serialize(embedding);

            var cmd = _sql.CreateCommand();
            cmd.CommandText = @"
                UPDATE Documents
                SET Embedding = @Embedding, EmbeddingModel = @ModelVersion, EmbeddingTimestamp = @Now
                WHERE Hash = @Hash";

            cmd.Parameters.AddWithValue("@Embedding", jsonEmbedding);
            cmd.Parameters.AddWithValue("@ModelVersion", modelVersion);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@Hash", hash);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}