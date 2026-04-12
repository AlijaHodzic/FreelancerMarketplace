using Freelance.Application.DTOs.Admin;
using Freelance.Application.Interfaces;
using Freelance.Domain.Enums;
using Freelance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _db;

    public AdminDashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AdminSummaryDto> GetSummaryAsync()
    {
        var users = _db.Users.AsNoTracking();
        var projects = _db.Projects.AsNoTracking();
        var bids = _db.Bids.AsNoTracking();

        return new AdminSummaryDto
        {
            TotalUsers = await users.CountAsync(),
            TotalClients = await users.CountAsync(user => user.Role == UserRole.Client),
            TotalFreelancers = await users.CountAsync(user => user.Role == UserRole.Freelancer),
            TotalAdmins = await users.CountAsync(user => user.Role == UserRole.Admin),
            TotalProjects = await projects.CountAsync(),
            OpenProjects = await projects.CountAsync(project => project.Status == ProjectStatus.Open),
            InProgressProjects = await projects.CountAsync(project => project.Status == ProjectStatus.InProgress),
            CompletedProjects = await projects.CountAsync(project => project.Status == ProjectStatus.Completed),
            TotalBids = await bids.CountAsync(),
            PendingBids = await bids.CountAsync(bid => bid.Status == BidStatus.Pending),
            AcceptedBids = await bids.CountAsync(bid => bid.Status == BidStatus.Accepted),
            RejectedBids = await bids.CountAsync(bid => bid.Status == BidStatus.Rejected),
            TotalConversations = await _db.Conversations.CountAsync(),
            TotalMessages = await _db.Messages.CountAsync(),
            TotalReviews = await _db.Reviews.CountAsync(),
            TotalNotifications = await _db.Notifications.CountAsync(),
            RecentUsers = await users
                .OrderByDescending(user => user.CreatedAtUtc)
                .Take(6)
                .Select(user => new AdminUserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    Location = user.Location,
                    CreatedAtUtc = user.CreatedAtUtc,
                })
                .ToListAsync(),
            RecentProjects = await projects
                .Include(project => project.Owner)
                .Include(project => project.Bids)
                .OrderByDescending(project => project.CreatedAtUtc)
                .Take(6)
                .Select(project => new AdminProjectDto
                {
                    Id = project.Id,
                    Title = project.Title,
                    ClientName = project.Owner.FullName,
                    Status = project.Status,
                    BudgetMin = project.BudgetMin,
                    BudgetMax = project.BudgetMax,
                    BidsCount = project.Bids.Count,
                    CreatedAtUtc = project.CreatedAtUtc,
                })
                .ToListAsync(),
        };
    }
}
