using System.Security.Cryptography;
using System.Text;

public static class WebhookSigner
{
    public static string Sign(string content, byte[] privateKeyBytes)
    {
        using var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

        byte[] data = Encoding.UTF8.GetBytes(content);
        byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signature);
    }
}