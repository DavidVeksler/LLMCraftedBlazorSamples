using Microsoft.EntityFrameworkCore;
using LLMCraftedBlazorSamples.Shared.Models;

namespace LLMCraftedBlazorSamples.Shared.Data;

/// <summary>
/// Database context for accessing Oracle ZOCLINIC schema
/// </summary>
public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
        : base(options)
    {
    }

    public DbSet<Group> Groups { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Group entity
        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("GROUPS_OLD", "ZOCLINIC");
            entity.HasKey(e => e.GroupId);

            // Oracle typically uses uppercase column names
            entity.Property(e => e.GroupId).HasColumnName("GROUP_ID");
            entity.Property(e => e.GroupCode).HasColumnName("GROUP_CODE");
            entity.Property(e => e.GroupName).HasColumnName("GROUP_NAME");
            entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
            entity.Property(e => e.CreatedDate).HasColumnName("CREATED_DATE");
            entity.Property(e => e.LastUpdate).HasColumnName("LAST_UPDATE");
            entity.Property(e => e.CompanyId).HasColumnName("COMPANY_ID");
            entity.Property(e => e.Address1).HasColumnName("ADDRESS_1");
            entity.Property(e => e.Address2).HasColumnName("ADDRESS_2");
            entity.Property(e => e.City).HasColumnName("CITY");
            entity.Property(e => e.State).HasColumnName("STATE");
            entity.Property(e => e.Zip).HasColumnName("ZIP");
        });
    }
}
