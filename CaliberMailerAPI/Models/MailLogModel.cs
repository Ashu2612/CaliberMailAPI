using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaliberMailerAPI.Models
{
    [Keyless]
    public class MailLogModel
    {
        public int ProfileId { get; set; }
        [NotMapped]
        public List<string> EmailTo { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? MailBody { get; set; }
        [NotMapped]
        public List<string> CCMail { get; set; } = new List<string>();

        [NotMapped]
        public List<byte[]> AttachmentFileBytes { get; set; } = new List<byte[]>();

        [NotMapped]
        public List<string> AttachmentFileNames { get; set; } = new List<string>();
        public string Status { get; set; }
        public string Error { get; set; }
        public DateTime DateTime { get; set; }

    }
}
