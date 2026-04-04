using Freelance.Application.DTOs.Freelancers;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Freelance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Services;

public class FavoriteFreelancerService : IFavoriteFreelancerService
{
    private readonly AppDbContext _db;
    private readonly IFreelancerProfileService _freelancerProfileService;

    public FavoriteFreelancerService(AppDbContext db, IFreelancerProfileService freelancerProfileService)
    {
        _db = db;
        _freelancerProfileService = freelancerProfileService;
    }

    public async Task<IEnumerable<FreelancerSummaryDto>> GetMineAsync(Guid clientId)
    {
        var favorites = await _db.FavoriteFreelancers
            .Include(favorite => favorite.Freelancer)
            .Where(favorite => favorite.ClientId == clientId)
            .OrderByDescending(favorite => favorite.CreatedAtUtc)
            .ToListAsync();

        return favorites.Select(favorite => FreelancerProfileService.MapSummary(favorite.Freelancer));
    }

    public async Task AddAsync(Guid clientId, Guid freelancerId)
    {
        var client = await _db.Users.FirstOrDefaultAsync(user => user.Id == clientId);
        if (client == null || client.Role is not UserRole.Client and not UserRole.Admin)
        {
            throw new InvalidOperationException("Only clients can save freelancers");
        }

        var freelancer = await _db.Users.FirstOrDefaultAsync(user => user.Id == freelancerId && user.Role == UserRole.Freelancer);
        if (freelancer == null)
        {
            throw new InvalidOperationException("Freelancer not found");
        }

        var exists = await _db.FavoriteFreelancers.AnyAsync(favorite => favorite.ClientId == clientId && favorite.FreelancerId == freelancerId);
        if (exists)
        {
            return;
        }

        _db.FavoriteFreelancers.Add(new FavoriteFreelancer
        {
            ClientId = clientId,
            FreelancerId = freelancerId,
        });

        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(Guid clientId, Guid freelancerId)
    {
        var favorite = await _db.FavoriteFreelancers.FirstOrDefaultAsync(item => item.ClientId == clientId && item.FreelancerId == freelancerId);
        if (favorite == null)
        {
            return;
        }

        _db.FavoriteFreelancers.Remove(favorite);
        await _db.SaveChangesAsync();
    }
}
