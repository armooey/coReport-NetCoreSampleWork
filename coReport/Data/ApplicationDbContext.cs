using System;
using System.Collections.Generic;
using System.Text;
using coReport.Auth;
using coReport.Models.AccountModel;
using coReport.Models.LogModel;
using coReport.Models.ManagerModels;
using coReport.Models.MessageModels;
using coReport.Models.ProjectModels;
using coReport.Models.ReportModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace coReport.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<short>, short>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ManagerReport> ManagerReports { get; set; }
        public DbSet<ProjectManager> ProjectManagers { get; set; }
        public DbSet<UserManager> UserManagers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserMessage> UserMessages { get; set; }
        public DbSet<ProfileImageHistory> ProfileImageHistories { get; set; }
        public DbSet<ReportAttachmentHistory> ReportAttachmentHistories { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(a => a.Reports)
                .WithOne(r => r.Author)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(a => a.ManagerReports)
                .WithOne(mr => mr.Author)
                .OnDelete(DeleteBehavior.Cascade);

            //Project and Report Relation Configuration
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Reports)
                .WithOne(r => r.Project);

            //Report and Project manager relation
            modelBuilder.Entity<ProjectManager>()
                .HasKey(pm => new { pm.ManagerId, pm.ReportId });

            modelBuilder.Entity<ProjectManager>()
                .HasOne(pm => pm.Manager)
                .WithMany(m => m.ProjectsManaged)
                .HasForeignKey(pm => pm.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectManager>()
                .HasOne(pm => pm.Report)
                .WithMany(r => r.ProjectManagers)
                .HasForeignKey(pm => pm.ReportId)
                .OnDelete(DeleteBehavior.Restrict);

            //relation between MangerReport and Report
            modelBuilder.Entity<ManagerReport>()
                .HasOne(mr => mr.Report)
                .WithMany(r => r.ManagerReports)
                .OnDelete(DeleteBehavior.Restrict);

            //Configuring User-Manager Many To Many relation
            modelBuilder.Entity<UserManager>()
                .HasKey(um => new { um.ManagerId, um.UserId });

            modelBuilder.Entity<UserManager>()
                .HasOne(um => um.User)
                .WithMany(u => u.Managers)
                .HasForeignKey(um => um.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserManager>()
                .HasOne(um => um.Manager)
                .WithMany(m => m.Users)
                .HasForeignKey(um => um.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            //Relation between Message and sender
            modelBuilder.Entity<ApplicationUser>()
               .HasMany(a => a.SentMessages)
               .WithOne(m => m.Sender)
               .OnDelete(DeleteBehavior.Cascade);

            //Relation between Message and Receivers
            modelBuilder.Entity<UserMessage>()
                .HasKey(um => new { um.MessageId, um.ReceiverId });

            modelBuilder.Entity<UserMessage>()
                .HasOne(um => um.Message)
                .WithMany(m => m.Receivers)
                .HasForeignKey(um => um.MessageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserMessage>()
                .HasOne(um => um.Receiver)
                .WithMany(r => r.ReceivedMessages)
                .HasForeignKey(um => um.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
