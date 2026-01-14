using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freelance.Infrastructure.Data.Config;

public class ProjectConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.ToTable("Projects");

        b.HasKey(x => x.Id);

        b.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Description)
            .HasMaxLength(4000)
            .IsRequired();

        b.Property(x => x.BudgetMin)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.BudgetMax)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.Status).IsRequired();
        b.Property(x => x.CreatedAtUtc).IsRequired();

        b.HasOne(x => x.Client)
            .WithMany(u => u.Projects)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Bids)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
