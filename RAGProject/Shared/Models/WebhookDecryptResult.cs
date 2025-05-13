namespace Spt.Rag.Shared.Models
{
    public class WebhookDecryptResult
    {
        public string DecryptedBody { get; set; }
        public bool SignatureValid { get; set; }
        public string KeyVersion { get; set; }
        public string TenantId { get; set; }
        public string Timestamp { get; set; }
        public string ReplayedTo { get; set; }
        public string DiagnosticId { get; set; }
    }
}