using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaliberMailerAPI.Models
{
    [Keyless]
    public class MailsModel
    {
        public int ProfileId { get; set; }
        public List<string> EmailTo { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? MailBody { get; set; }
        public List<string> CCMail { get; set; } = new List<string>();

        [NotMapped]
        public List<byte[]> AttachmentFileBytes { get; set; } = new List<byte[]>();
        public List<string> AttachmentFileNames { get; set; } = new List<string>();
    }
}
