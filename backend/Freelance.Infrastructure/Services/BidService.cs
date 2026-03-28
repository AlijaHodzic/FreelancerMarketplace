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

    public BidService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<BidResponse> CreateAsync(Guid freelancerId, CreateBidRequest request)
    {
        var project = await _db.Projects
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

        return MapToResponse(bid, project);
    }

    public async Task<IEnumerable<BidResponse>> GetByProjectAsync(Guid projectId, Guid requesterId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            throw new InvalidOperationException("Project not found");

        if (project.OwnerId != requesterId)
            throw new InvalidOperationException("Access denied");

        var bids = await _db.Bids
            .Where(b => b.ProjectId == projectId)
            .OrderBy(b => b.CreatedAtUtc)
            .ToListAsync();

        return bids.Select(b => MapToResponse(b, project));
    }

    public async Task<IEnumerable<BidResponse>> GetMineAsync(Guid freelancerId)
    {
        var bids = await _db.Bids
            .Include(b => b.Project)
            .Where(b => b.FreelancerId == freelancerId)
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToListAsync();

        return bids.Select(b => MapToResponse(b, b.Project));
    }

    public async Task AcceptAsync(Guid bidId, Guid ownerId)
    {
        var bid = await _db.Bids
            .Include(b => b.Project)
            .FirstOrDefaultAsync(b => b.Id == bidId);

        if (bid == null)
            throw new InvalidOperationException("Bid not found");

        if (bid.Project.OwnerId != ownerId)
            throw new InvalidOperationException("Access denied");

        bid.Status = BidStatus.Accepted;
        bid.Project.Status = ProjectStatus.InProgress;

        var otherBids = await _db.Bids
            .Where(b => b.ProjectId == bid.ProjectId && b.Id != bid.Id)
            .ToListAsync();

        foreach (var other in otherBids)
            other.Status = BidStatus.Rejected;

        await _db.SaveChangesAsync();
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
    }

    private static BidResponse MapToResponse(Bid bid, Project project)
    {
        return new BidResponse
        {
            Id = bid.Id,
            ProjectId = bid.ProjectId,
            FreelancerId = bid.FreelancerId,
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
