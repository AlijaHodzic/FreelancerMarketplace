using Freelance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<FavoriteFreelancer> FavoriteFreelancers => Set<FavoriteFreelancer>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Conversation>()
            .HasOne(conversation => conversation.Client)
            .WithMany(user => user.ClientConversations)
            .HasForeignKey(conversation => conversation.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Conversation>()
            .HasOne(conversation => conversation.Freelancer)
            .WithMany(user => user.FreelancerConversations)
            .HasForeignKey(conversation => conversation.FreelancerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(message => message.Sender)
            .WithMany(user => user.Messages)
            .HasForeignKey(message => message.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FavoriteFreelancer>()
            .HasOne(favorite => favorite.Client)
            .WithMany(user => user.SavedFreelancers)
            .HasForeignKey(favorite => favorite.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FavoriteFreelancer>()
            .HasOne(favorite => favorite.Freelancer)
            .WithMany(user => user.SavedByClients)
            .HasForeignKey(favorite => favorite.FreelancerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FavoriteFreelancer>()
            .HasIndex(favorite => new { favorite.ClientId, favorite.FreelancerId })
            .IsUnique();

        modelBuilder.Entity<Project>()
            .HasOne(project => project.AssignedFreelancer)
            .WithMany(user => user.AssignedProjects)
            .HasForeignKey(project => project.AssignedFreelancerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(review => review.Project)
            .WithMany(project => project.Reviews)
            .HasForeignKey(review => review.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(review => review.Client)
            .WithMany(user => user.ReviewsGiven)
            .HasForeignKey(review => review.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(review => review.Freelancer)
            .WithMany(user => user.ReviewsReceived)
            .HasForeignKey(review => review.FreelancerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasIndex(review => review.ProjectId)
            .IsUnique();

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.User)
            .WithMany(user => user.Notifications)
            .HasForeignKey(notification => notification.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
