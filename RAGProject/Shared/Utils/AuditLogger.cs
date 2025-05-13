using Microsoft.Data.SqlClient;
using Spt.Rag.Shared.Models;

namespace Spt.Rag.Shared.Utils
{
    public static class AuditLogger
    {
        public static async Task LogDuplicate(SqlConnection sql, DocumentUploadMessage upload, string hash)
        {
            var command = sql.CreateCommand();
            command.CommandText = @"
                INSERT INTO DeduplicationEvents (FileName, Hash, UploadedBy, EventTime, Reason)
                VALUES (@FileName, @Hash, @UploadedBy, @EventTime, @Reason)";

            command.Parameters.AddWithValue("@FileName", upload.FileName);
            command.Parameters.AddWithValue("@Hash", hash);
            command.Parameters.AddWithValue("@UploadedBy", upload.UploadedBy);
            command.Parameters.AddWithValue("@EventTime", DateTime.UtcNow);
            command.Parameters.AddWithValue("@Reason", "Duplicate");

            await command.ExecuteNonQueryAsync();
        }

        public static async Task LogEmbeddingRejection(SqlConnection sql, object embeddingRequest)
        {
            throw new NotImplementedException();
        }
    }
}
