using System.ComponentModel.DataAnnotations;

namespace Spt.Rag.Shared.Models
{
    public class WebhookDecryptRequest
    {
        [Required]
        public string EncryptedPayload { get; set; }

        [Required]
        public string Signature { get; set; }

        public string ReplayTo { get; set; }
    }
}