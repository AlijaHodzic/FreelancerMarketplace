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

    public async Task<List<AdminUserDto>> GetUsersAsync()
    {
        return await _db.Users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAtUtc)
            .Select(user => new AdminUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Location = user.Location,
                CreatedAtUtc = user.CreatedAtUtc,
            })
            .ToListAsync();
    }

    public async Task<List<AdminProjectDto>> GetProjectsAsync()
    {
        return await _db.Projects
            .AsNoTracking()
            .Include(project => project.Owner)
            .Include(project => project.Bids)
            .OrderByDescending(project => project.CreatedAtUtc)
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
            .ToListAsync();
    }

    public async Task<List<AdminActivityDto>> GetActivityAsync()
    {
        var userEvents = await _db.Users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAtUtc)
            .Take(10)
            .Select(user => new AdminActivityDto
            {
                Type = "user",
                Title = "New user joined",
                Description = $"{user.FullName} joined as {user.Role}.",
                CreatedAtUtc = user.CreatedAtUtc,
            })
            .ToListAsync();

        var projectEvents = await _db.Projects
            .AsNoTracking()
            .Include(project => project.Owner)
            .OrderByDescending(project => project.CreatedAtUtc)
            .Take(10)
            .Select(project => new AdminActivityDto
            {
                Type = "project",
                Title = "Project posted",
                Description = $"{project.Owner.FullName} posted \"{project.Title}\".",
                CreatedAtUtc = project.CreatedAtUtc,
            })
            .ToListAsync();

        var bidEvents = await _db.Bids
            .AsNoTracking()
            .Include(bid => bid.Project)
            .Include(bid => bid.Freelancer)
            .OrderByDescending(bid => bid.CreatedAtUtc)
            .Take(10)
            .Select(bid => new AdminActivityDto
            {
                Type = "bid",
                Title = "Proposal submitted",
                Description = $"{bid.Freelancer.FullName} submitted a proposal for \"{bid.Project.Title}\".",
                CreatedAtUtc = bid.CreatedAtUtc,
            })
            .ToListAsync();

        var messageEvents = await _db.Messages
            .AsNoTracking()
            .Include(message => message.Sender)
            .OrderByDescending(message => message.CreatedAtUtc)
            .Take(10)
            .Select(message => new AdminActivityDto
            {
                Type = "message",
                Title = "New message sent",
                Description = $"{message.Sender.FullName} sent a new conversation message.",
                CreatedAtUtc = message.CreatedAtUtc,
            })
            .ToListAsync();

        var reviewEvents = await _db.Reviews
            .AsNoTracking()
            .Include(review => review.Client)
            .Include(review => review.Freelancer)
            .OrderByDescending(review => review.CreatedAtUtc)
            .Take(10)
            .Select(review => new AdminActivityDto
            {
                Type = "review",
                Title = "Review published",
                Description = $"{review.Client.FullName} reviewed {review.Freelancer.FullName}.",
                CreatedAtUtc = review.CreatedAtUtc,
            })
            .ToListAsync();

        return userEvents
            .Concat(projectEvents)
            .Concat(bidEvents)
            .Concat(messageEvents)
            .Concat(reviewEvents)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(18)
            .ToList();
    }
}
