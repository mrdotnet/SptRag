using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

string tenantId = "test-tenant-123";
string keyName = $"webhook-decryption-key-{tenantId}";
string keyVaultUrl = "https://<your-key-vault-name>.vault.azure.net/";

// Sample payload
var payload = new
{
    eventType = "document.uploaded",
    documentId = Guid.NewGuid().ToString(),
    timestamp = DateTime.UtcNow
};
string jsonBody = JsonConvert.SerializeObject(payload);
Console.WriteLine("Original Payload:");
Console.WriteLine(jsonBody);

// Setup Key Vault clients
var credential = new DefaultAzureCredential();
var keyClient = new KeyClient(new Uri(keyVaultUrl), credential);
var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);

// Retrieve key
KeyVaultKey key = await keyClient.GetKeyAsync(keyName);
var cryptoClient = new CryptographyClient(key.Id, credential);

// Encrypt
byte[] plainBytes = Encoding.UTF8.GetBytes(jsonBody);
EncryptResult encrypted = await cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, plainBytes);

// Sign
using var rsa = RSA.Create();
rsa.ImportRSAPublicKey(Convert.FromBase64String(secretClient.GetSecret($"{keyName}-public").Value.Value), out _);
byte[] hash = SHA256.HashData(plainBytes);
byte[] signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

// Output test payload
var testPayload = new
{
    EncryptedPayload = Convert.ToBase64String(encrypted.Ciphertext),
    Signature = Convert.ToBase64String(signature),
    ReplayTo = "https://your-function-url/api/your-endpoint"
};

Console.WriteLine("\nUse this payload in your webhook decrypt API test:");
Console.WriteLine(JsonConvert.SerializeObject(testPayload, Formatting.Indented));