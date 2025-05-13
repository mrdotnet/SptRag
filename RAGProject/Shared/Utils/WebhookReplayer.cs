using System.Text;

namespace Spt.Rag.Shared.Utils
{
    public static class WebhookReplayer
    {
        public static async Task<WebhookReplayResult> TriggerReplayAsync(string id, string overridePayload = null)
        {
            var original = await WebhookStore.GetRecordAsync(id);
            if (original == null) return new WebhookReplayResult { Success = false, Status = "NotFound" };

            var payload = overridePayload ?? original.Payload;
            var client = new HttpClient();

            try
            {
                var res = await client.PostAsync(original.Endpoint,
                    new StringContent(payload, Encoding.UTF8, "application/json"));

                var success = res.IsSuccessStatusCode;
                await WebhookStore.RecordReplayAttempt(id, payload, success);

                return new WebhookReplayResult
                {
                    Success = success,
                    Status = res.StatusCode.ToString(),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await WebhookStore.RecordReplayAttempt(id, payload, false);
                await TelemetryLogger.LogWebhookException(id, ex);
                return new WebhookReplayResult { Success = false, Status = "Error", Timestamp = DateTime.UtcNow };
            }
        }
    }

    public class WebhookReplayResult
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
    }
}