namespace Spt.Rag.Shared.Utils
{
    public static class HashUtils
    {
        public static string ComputeBlake3(string base64Content)
        {
            var bytes = Convert.FromBase64String(base64Content);
            using var hasher = Blake3.Hasher.New();
            hasher.Update(bytes);
            return hasher.Finalize().ToString();
        }
        public static string ComputeSha256(string base64Content)
        {
            var bytes = Convert.FromBase64String(base64Content);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

}
