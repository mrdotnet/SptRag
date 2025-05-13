using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace SptRAG.Functions
{
    public class WebhookReplayApi
    {
        private readonly ILogger _logger;

        public WebhookReplayApi(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WebhookReplayApi>();
        }

        [Function("GetFailedWebhooks")]
        public async Task<HttpResponseData> GetFailedWebhooks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "webhooks/failed")] HttpRequestData req)
        {
            if (!await RbacChecker.HasAccess(req, "webhook.list")) return req.CreateResponse(HttpStatusCode.Forbidden);

            var failed = await WebhookStore.ListFailedDeliveriesAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(failed);
            return response;
        }

        [Function("GetWebhookPayload")]
        public async Task<HttpResponseData> GetWebhookPayload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "webhooks/payload/{id}")] HttpRequestData req,
            string id)
        {
            if (!await RbacChecker.HasAccess(req, "webhook.read")) return req.CreateResponse(HttpStatusCode.Forbidden);

            var payload = await WebhookStore.GetPayloadAsync(id);
            if (payload == null) return req.CreateResponse(HttpStatusCode.NotFound);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(payload);
            return response;
        }

        [Function("ReplayWebhook")]
        public async Task<HttpResponseData> ReplayWebhook(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhooks/replay/{id}")] HttpRequestData req,
            string id)
        {
            if (!await RbacChecker.HasAccess(req, "webhook.replay")) return req.CreateResponse(HttpStatusCode.Forbidden);

            var body = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(req.Body);
            var overridePayload = body.ContainsKey("payload") ? JsonSerializer.Serialize(body["payload"]) : null;

            var replayResult = await WebhookReplayer.TriggerReplayAsync(id, overridePayload);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                Success = replayResult.Success,
                Status = replayResult.Status,
                Timestamp = replayResult.Timestamp
            });

            return response;
        }
    }
}
