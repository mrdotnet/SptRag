using Microsoft.Data.SqlClient;

namespace Spt.Rag.Shared.Utils
{
    public static class SqlHelper
    {
        public static async Task StoreEmbeddingAsync(SqlConnection connection, string hash, string embeddingJson, string modelVersion)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Embeddings (Hash, Vector, ModelVersion, EmbeddedAt)
                VALUES (@Hash, @Vector, @ModelVersion, @EmbeddedAt)";

            cmd.Parameters.AddWithValue("@Hash", hash);
            cmd.Parameters.AddWithValue("@Vector", embeddingJson);
            cmd.Parameters.AddWithValue("@ModelVersion", modelVersion);
            cmd.Parameters.AddWithValue("@EmbeddedAt", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task StoreAuditEventAsync(SqlConnection connection, string category, string type, string payloadJson)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO AuditLog (Category, EventType, PayloadJson, CreatedAt)
                VALUES (@Category, @EventType, @PayloadJson, @CreatedAt)";

            cmd.Parameters.AddWithValue("@Category", category);
            cmd.Parameters.AddWithValue("@EventType", type);
            cmd.Parameters.AddWithValue("@PayloadJson", payloadJson);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}