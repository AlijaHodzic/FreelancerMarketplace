using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Freelance.Infrastructure.Data.Config;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");

        b.HasKey(x => x.Id);

        b.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        b.HasIndex(x => x.Email).IsUnique();

        b.Property(x => x.FullName)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Slug)
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty)
            .IsRequired();

        b.HasIndex(x => x.Slug)
            .IsUnique();

        b.Property(x => x.PasswordHash)
            .IsRequired();

        b.Property(x => x.Headline).HasMaxLength(200).HasDefaultValue(string.Empty).IsRequired();
        b.Property(x => x.AvatarUrl).HasMaxLength(500).HasDefaultValue(string.Empty).IsRequired();
        b.Property(x => x.Location).HasMaxLength(200).HasDefaultValue(string.Empty).IsRequired();
        b.Property(x => x.Category).HasMaxLength(120).HasDefaultValue(string.Empty).IsRequired();
        b.Property(x => x.ExperienceLevel).HasMaxLength(120).HasDefaultValue(string.Empty).IsRequired();
        b.Property(x => x.ResponseTime).HasMaxLength(120).HasDefaultValue(string.Empty).IsRequired();
        b.Property(x => x.SuccessRate).HasMaxLength(50).HasDefaultValue(string.Empty).IsRequired();
        b.Property(x => x.ShortDescription).HasMaxLength(500).HasDefaultValue(string.Empty).IsRequired();
        b.Property(x => x.About).HasMaxLength(4000).HasDefaultValue(string.Empty).IsRequired();
        b.Property(x => x.SkillsJson).HasDefaultValue("[]").IsRequired();
        b.Property(x => x.PortfolioJson).HasDefaultValue("[]").IsRequired();
        b.Property(x => x.TestimonialsJson).HasDefaultValue("[]").IsRequired();

        b.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
