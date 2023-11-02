﻿using Microsoft.EntityFrameworkCore;
using CaliberMailerAPI.Models;
using Microsoft.Graph;

namespace CaliberMailerAPI.Data
{
    public partial class MailLogContext : DbContext
    {
        public static dynamic configuration = new ConfigurationBuilder()
.SetBasePath(System.IO.Directory.GetCurrentDirectory())
.AddJsonFile("appsettings.json")
.Build();

        public static string connectionstring = configuration["DefaultConnection"];

        public MailLogContext() { }
        public MailLogContext(DbContextOptions<MailLogContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseSqlite(connectionstring);

        public DbSet<MailLogModel> AD_MAIL_LOG { get; set; }
        public DbSet<ProfilesModel> AD_MAIL_PROFILE { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProfilesModel>(entity =>
            {
                entity.HasKey(e => e.ProfileId);
            });
            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
