using Freelance.Application.DTOs.Bids;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Freelance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Services;

public class BidService : IBidService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public BidService(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<BidResponse> CreateAsync(Guid freelancerId, CreateBidRequest request)
    {
        var project = await _db.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId);

        if (project == null)
            throw new InvalidOperationException("Project not found");

        if (project.OwnerId == freelancerId)
            throw new InvalidOperationException("You cannot bid on your own project");

        if (project.Status != ProjectStatus.Open)
            throw new InvalidOperationException("Project is not open for bidding");

        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            FreelancerId = freelancerId,
            Amount = request.Amount,
            Message = request.Message,
            Status = BidStatus.Pending
        };

        _db.Bids.Add(bid);
        await _db.SaveChangesAsync();

        var freelancer = await _db.Users.FirstAsync(user => user.Id == freelancerId);
        await _notificationService.CreateAsync(
            project.OwnerId,
            "bid-received",
            "New proposal received",
            $"{freelancer.FullName} sent a proposal for \"{project.Title}\".",
            "/client-dashboard");
        return MapToResponse(bid, project, freelancer, project.Owner);
    }

    public async Task<IEnumerable<BidResponse>> GetByProjectAsync(Guid projectId, Guid requesterId)
    {
        var project = await _db.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            throw new InvalidOperationException("Project not found");

        if (project.OwnerId != requesterId)
            throw new InvalidOperationException("Access denied");

        var bids = await _db.Bids
            .Include(b => b.Freelancer)
            .Where(b => b.ProjectId == projectId)
            .OrderBy(b => b.CreatedAtUtc)
            .ToListAsync();

        return bids.Select(b => MapToResponse(b, project, b.Freelancer, project.Owner));
    }

    public async Task<IEnumerable<BidResponse>> GetMineAsync(Guid freelancerId)
    {
        var bids = await _db.Bids
            .Include(b => b.Project)
                .ThenInclude(project => project.Owner)
            .Include(b => b.Freelancer)
            .Where(b => b.FreelancerId == freelancerId)
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToListAsync();

        return bids.Select(b => MapToResponse(b, b.Project, b.Freelancer, b.Project.Owner));
    }

    public async Task AcceptAsync(Guid bidId, Guid ownerId)
    {
        var bid = await _db.Bids
            .Include(b => b.Project)
            .Include(b => b.Freelancer)
            .FirstOrDefaultAsync(b => b.Id == bidId);

        if (bid == null)
            throw new InvalidOperationException("Bid not found");

        if (bid.Project.OwnerId != ownerId)
            throw new InvalidOperationException("Access denied");

        if (bid.Project.Status == ProjectStatus.Completed || bid.Project.Status == ProjectStatus.Cancelled)
            throw new InvalidOperationException("This project can no longer accept proposals");

        if (bid.Project.AssignedFreelancerId != null && bid.Project.AssignedFreelancerId != bid.FreelancerId)
            throw new InvalidOperationException("This project already has a hired freelancer");

        bid.Status = BidStatus.Accepted;
        bid.Project.Status = ProjectStatus.InProgress;
        bid.Project.AssignedFreelancerId = bid.FreelancerId;
        bid.Project.HiredAtUtc = DateTime.UtcNow;
        bid.Project.CompletedAtUtc = null;
        bid.Project.UpdatedAtUtc = DateTime.UtcNow;

        var otherBids = await _db.Bids
            .Where(b => b.ProjectId == bid.ProjectId && b.Id != bid.Id)
            .ToListAsync();

        foreach (var other in otherBids)
            other.Status = BidStatus.Rejected;

        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(
            bid.FreelancerId,
            "bid-accepted",
            "Proposal accepted",
            $"Your proposal for \"{bid.Project.Title}\" was accepted.",
            "/my-applications");

        foreach (var other in otherBids)
        {
            await _notificationService.CreateAsync(
                other.FreelancerId,
                "bid-rejected",
                "Proposal closed",
                $"Another freelancer was selected for \"{bid.Project.Title}\".",
                "/my-applications");
        }
    }

    public async Task RejectAsync(Guid bidId, Guid ownerId)
    {
        var bid = await _db.Bids
            .Include(b => b.Project)
            .FirstOrDefaultAsync(b => b.Id == bidId);

        if (bid == null)
            throw new InvalidOperationException("Bid not found");

        if (bid.Project.OwnerId != ownerId)
            throw new InvalidOperationException("Access denied");

        bid.Status = BidStatus.Rejected;
        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(
            bid.FreelancerId,
            "bid-rejected",
            "Proposal rejected",
            $"Your proposal for \"{bid.Project.Title}\" was not selected.",
            "/my-applications");
    }

    private static BidResponse MapToResponse(Bid bid, Project project, User freelancer, User client)
    {
        return new BidResponse
        {
            Id = bid.Id,
            ProjectId = bid.ProjectId,
            FreelancerId = bid.FreelancerId,
            FreelancerName = freelancer.FullName,
            FreelancerEmail = freelancer.Email,
            ClientId = client.Id,
            ClientName = client.FullName,
            ClientEmail = client.Email,
            ProjectTitle = project.Title,
            ProjectDescription = project.Description,
            ProjectBudgetMin = project.BudgetMin,
            ProjectBudgetMax = project.BudgetMax,
            Amount = bid.Amount,
            Message = bid.Message,
            Status = bid.Status,
            CreatedAtUtc = bid.CreatedAtUtc
        };
    }
}
