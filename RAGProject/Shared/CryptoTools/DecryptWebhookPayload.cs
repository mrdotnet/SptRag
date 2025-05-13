using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace Spt.Rag.Shared.CryptoTools
{
    public class DecryptWebhookPayload
    {
        public static async Task Main(string[] args)
        {
            string keyVaultUrl = Environment.GetEnvironmentVariable("KEYVAULT_URI");
            string keyName = Environment.GetEnvironmentVariable("KEY_NAME");

            var client = new KeyClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultKey key = await client.GetKeyAsync(keyName);
            var cryptoClient = new CryptographyClient(key.Id, new DefaultAzureCredential());

            Console.Write("Paste base64 payload: ");
            string base64 = Console.ReadLine();

            byte[] encryptedData = Convert.FromBase64String(base64);
            DecryptResult decResult = await cryptoClient.DecryptAsync(EncryptionAlgorithm.RsaOaep256, encryptedData);

            string json = Encoding.UTF8.GetString(decResult.Plaintext);
            Console.WriteLine("Decrypted Payload:\n" + json);
        }
    }
}