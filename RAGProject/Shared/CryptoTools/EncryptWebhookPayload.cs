using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace Spt.Rag.Shared.CryptoTools
{
    public class EncryptWebhookPayload
    {
        public static async Task Main(string[] args)
        {
            string keyVaultUrl = Environment.GetEnvironmentVariable("KEYVAULT_URI");
            string keyName = Environment.GetEnvironmentVariable("KEY_NAME");
            EncryptionMode mode = (args.Length > 0 && args[0] == "aes") ? EncryptionMode.AesGcm256 : EncryptionMode.RsaOaep256;

            var client = new KeyClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultKey key = await client.GetKeyAsync(keyName);
            var cryptoClient = new CryptographyClient(key.Id, new DefaultAzureCredential());

            var payload = new
            {
                Event = "document.ingested",
                Timestamp = DateTime.UtcNow,
                FileName = "invoice-2023.pdf",
                Hash = "abc123def456"
            };

            string json = JsonSerializer.Serialize(payload);
            byte[] plaintext = Encoding.UTF8.GetBytes(json);

            EncryptResult encResult;
            if (mode == EncryptionMode.RsaOaep256)
            {
                encResult = await cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep256, plaintext);
            }
            else
            {
                encResult = await cryptoClient.EncryptAsync(EncryptionAlgorithm.A256Gcm, plaintext);
            }
            string base64 = Convert.ToBase64String(encResult.Ciphertext);

            Console.WriteLine(base64);
        }
    }
}