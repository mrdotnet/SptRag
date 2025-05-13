using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public static class PayloadEncryptor
{
    public static string Encrypt(string jsonPayload, byte[] aesKey, byte[] aesIV)
    {
        using var aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = aesIV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var encryptor = aes.CreateEncryptor();
        byte[] inputBytes = Encoding.UTF8.GetBytes(jsonPayload);
        byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

        return Convert.ToBase64String(encrypted);
    }

    public static (byte[] Key, byte[] IV) GenerateSymmetricKey()
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        return (aes.Key, aes.IV);
    }
}