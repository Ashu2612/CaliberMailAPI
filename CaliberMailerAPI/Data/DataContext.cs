using Microsoft.EntityFrameworkCore;
using CaliberMailerAPI.Models;
using Microsoft.Graph;

namespace CaliberMailerAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<MailsModel> AD_MAIL_LOG { get; set; }
        public DbSet<ProfilesModel> AD_MAIL_PROFILE { get; set; }

        
    }
}
