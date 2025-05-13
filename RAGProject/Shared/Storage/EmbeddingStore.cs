using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Spt.Rag.Shared.Storage;

public static class EmbeddingStore
{
    public static async Task SaveEmbedding(SqlConnection sql, string hash, string model, float[] vector,
        DateTime embeddedAt)
    {
        var command = sql.CreateCommand();
        command.CommandText = @"
                INSERT INTO Embeddings (Hash, Model, Vector, EmbeddedAt)
                VALUES (@Hash, @Model, @Vector, @EmbeddedAt)";

        command.Parameters.AddWithValue("@Hash", hash);
        command.Parameters.AddWithValue("@Model", model);
        command.Parameters.AddWithValue("@Vector", JsonConvert.SerializeObject(vector));
        command.Parameters.AddWithValue("@EmbeddedAt", embeddedAt);

        await command.ExecuteNonQueryAsync();

        var auditCommand = sql.CreateCommand();
        auditCommand.CommandText = @"
                INSERT INTO EmbeddingAudit (Hash, Model, VectorSnapshot, Timestamp)
                VALUES (@Hash, @Model, @Snapshot, @Timestamp)";

        auditCommand.Parameters.AddWithValue("@Hash", hash);
        auditCommand.Parameters.AddWithValue("@Model", model);
        auditCommand.Parameters.AddWithValue("@Snapshot", JsonConvert.SerializeObject(vector));
        auditCommand.Parameters.AddWithValue("@Timestamp", embeddedAt);

        await auditCommand.ExecuteNonQueryAsync();
    }
}