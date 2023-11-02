using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CaliberMailerAPI.Models
{
    public class ProfilesModel
    {
        [Key]
        public int ProfileId { get; set; }
        public string? SMTPServer { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? TenantId { get; set; }
        public string? OfficeEmail { get; set; }
        public string? OfficeEmailPassword { get; set; }
    }
}
