using System.Globalization;
using CsvHelper;
using EfFuncCallSK.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EfFuncCallSK.Data;

public class ApplicationDbContext : IdentityDbContext // Assuming you have an ApplicationUser class
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Ensure the Identity models are correctly configured
        // Your custom model configurations can follow
        modelBuilder.Entity<ChatHistory>(entity =>
        {
        entity.HasOne(d => d.User)
              .WithMany()
              .HasForeignKey(d => d.UserId)
              .HasConstraintName("ForeignKey_ChatHistory_User");
        });
        modelBuilder.Entity<Resume>(entity =>
            {
                entity.HasOne(r => r.JobDescription);
            });
    }

    public DbSet<ChatHistory> ChatHistories { get; set; }
    public DbSet<Resume> Resumes { get; set; }
    public DbSet<JobDescription> JobDescriptions { get; set; }
    // public DbSet<CoverLetter> CoverLetters { get; set; }
}
