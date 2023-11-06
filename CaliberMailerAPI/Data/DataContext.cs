using CaliberMailerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CaliberMailerAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<MailLogModel> AD_MAIL_LOG { get; set; }
        public DbSet<ProfilesModel> AD_MAIL_PROFILE { get; set; }
    }
}
