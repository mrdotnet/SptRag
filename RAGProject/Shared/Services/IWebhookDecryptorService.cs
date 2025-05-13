using Microsoft.AspNetCore.Http;
using Spt.Rag.Shared.Models;

namespace Spt.Rag.Shared.Services
{
    public interface IWebhookDecryptorService
    {
        Task<WebhookDecryptResult> DecryptAsync(WebhookDecryptRequest request, HttpContext context);
    }
}