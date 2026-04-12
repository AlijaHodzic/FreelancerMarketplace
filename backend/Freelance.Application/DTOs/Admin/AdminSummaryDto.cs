namespace Freelance.Application.DTOs.Admin;

public class AdminSummaryDto
{
    public int TotalUsers { get; set; }
    public int TotalClients { get; set; }
    public int TotalFreelancers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalProjects { get; set; }
    public int OpenProjects { get; set; }
    public int InProgressProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int TotalBids { get; set; }
    public int PendingBids { get; set; }
    public int AcceptedBids { get; set; }
    public int RejectedBids { get; set; }
    public int TotalConversations { get; set; }
    public int TotalMessages { get; set; }
    public int TotalReviews { get; set; }
    public int TotalNotifications { get; set; }
    public List<AdminUserDto> RecentUsers { get; set; } = new();
    public List<AdminProjectDto> RecentProjects { get; set; } = new();
}
