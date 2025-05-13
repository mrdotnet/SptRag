using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SptRag.Admin.Client
{
    [Table("Documents", Schema = "dbo")]
    public partial class Document
    {
        [Key]
        [Column("id")]
        [Required]
        public string Id { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("embedding")]
        public byte[] Embedding { get; set; }
    }
}