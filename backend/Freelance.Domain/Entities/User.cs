using Freelance.Domain.Common;
using Freelance.Domain.Enums;

namespace Freelance.Domain.Entities
{
    public class User : AuditableEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Headline { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ExperienceLevel { get; set; } = string.Empty;
        public string ResponseTime { get; set; } = string.Empty;
        public string SuccessRate { get; set; } = string.Empty;
        public int CompletedProjects { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal Rating { get; set; }
        public int ReviewsCount { get; set; }
        public string ShortDescription { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;
        public string SkillsJson { get; set; } = "[]";
        public string PortfolioJson { get; set; } = "[]";
        public string TestimonialsJson { get; set; } = "[]";

        public UserRole Role { get; set; } = UserRole.Client;

        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Conversation> ClientConversations { get; set; } = new List<Conversation>();
        public ICollection<Conversation> FreelancerConversations { get; set; } = new List<Conversation>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<FavoriteFreelancer> SavedFreelancers { get; set; } = new List<FavoriteFreelancer>();
        public ICollection<FavoriteFreelancer> SavedByClients { get; set; } = new List<FavoriteFreelancer>();
        public ICollection<Project> AssignedProjects { get; set; } = new List<Project>();
        public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
        public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
