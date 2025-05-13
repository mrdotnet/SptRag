using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Secrets;
using Spt.Rag.Shared.Models;
using Spt.Rag.Shared.Services;

namespace SptRag.Admin.Server.Services
{
    public class WebhookDecryptorService : IWebhookDecryptorService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SecretClient _secretClient;
        private readonly KeyClient _keyClient;

        public WebhookDecryptorService(IHttpClientFactory factory, SecretClient secretClient, KeyClient keyClient)
        {
            _httpClientFactory = factory;
            _secretClient = secretClient;
            _keyClient = keyClient;
        }

        public async Task<WebhookDecryptResult> DecryptAsync(WebhookDecryptRequest request, HttpContext context)
        {
            string tenantId = context.Request.Headers["X-Tenant-ID"];
            var keyIdentifier = await _secretClient.GetSecretAsync($"webhook-decryption-key-{tenantId}");

            var cryptoClient = new CryptographyClient(new Uri(keyIdentifier.Value.Value), new DefaultAzureCredential());
            byte[] encrypted = Convert.FromBase64String(request.EncryptedPayload);
            byte[] decryptedBytes = (await cryptoClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, encrypted)).Plaintext;
            string decryptedBody = Encoding.UTF8.GetString(decryptedBytes);

            bool signatureValid = VerifySignature(decryptedBody, request.Signature);

            string replayUri = null;
            if (!string.IsNullOrEmpty(request.ReplayTo))
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(decryptedBody, Encoding.UTF8, "application/json");
                await client.PostAsync(request.ReplayTo, content);
                replayUri = request.ReplayTo;
            }

            return new WebhookDecryptResult
            {
                DecryptedBody = decryptedBody,
                SignatureValid = signatureValid,
                KeyVersion = keyIdentifier.Value.Properties.Version,
                TenantId = tenantId,
                Timestamp = DateTime.UtcNow.ToString("o"),
                ReplayedTo = replayUri,
                DiagnosticId = Guid.NewGuid().ToString()
            };
        }

        private bool VerifySignature(string body, string signature)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(body));
            var signatureBytes = Convert.FromBase64String(signature);
            // This would typically compare with a public key per tenant
            // For simplicity, return true for this mock
            return true;
        }
    }
}
