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

        b.Property(x => x.PasswordHash)
            .IsRequired();

        b.Property(x => x.CreatedAtUtc).IsRequired();

        
    }
}
