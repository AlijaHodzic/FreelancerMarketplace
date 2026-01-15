using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freelance.Infrastructure.Data.Config;

public class BidConfig : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.ToTable("Bids");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.Message)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired();

        // Relationship: Bid -> Project
        builder.HasOne(b => b.Project)
            .WithMany(p => p.Bids)
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship: Bid -> Freelancer (User)
        builder.HasOne(b => b.Freelancer)
            .WithMany(u => u.Bids)
            .HasForeignKey(b => b.FreelancerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes (performance)
        builder.HasIndex(b => b.ProjectId);
        builder.HasIndex(b => b.FreelancerId);
    }
}
