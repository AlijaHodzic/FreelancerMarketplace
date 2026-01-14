using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freelance.Infrastructure.Data.Config
{
    public class BidConfig : IEntityTypeConfiguration<Bid>
    {
        public void Configure(EntityTypeBuilder<Bid> b)
        {
            b.ToTable("Bids");

            b.HasKey(x => x.Id);

            b.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            b.Property(x => x.Message)
                .HasMaxLength(2000)
                .IsRequired();

            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Freelancer)
                .WithMany(u => u.Bids)
                .HasForeignKey(x => x.FreelancerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
