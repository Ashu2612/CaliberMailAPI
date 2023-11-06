using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaliberMailerAPI.Models
{
  
    public class MailLogModel
    {
        [Key]
        public int ProfileId { get; set; }
        public string? EmailTo { get; set; }
        public string? Subject { get; set; }
        public string? MailBody { get; set; }
        public string? CCMail { get; set; }
        public string? AttachmentFileBytes { get; set; }
        public string? AttachmentFileNames { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public DateTime DateTime { get; set; }

    }
}
