using Freelance.Application.DTOs.Clients;
using Freelance.Application.Interfaces;
using Freelance.Domain.Enums;
using Freelance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Services;

public class ClientProfileService : IClientProfileService
{
    private readonly AppDbContext _db;

    public ClientProfileService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ClientProfileDto> GetMineAsync(Guid userId)
    {
        var client = await _db.Users.FirstOrDefaultAsync(user =>
            user.Id == userId && (user.Role == UserRole.Client || user.Role == UserRole.Admin));

        if (client == null)
        {
            throw new InvalidOperationException("Client profile not found");
        }

        return Map(client);
    }

    public async Task<ClientProfileDto> UpdateAsync(Guid userId, UpdateClientProfileRequest request)
    {
        var client = await _db.Users.FirstOrDefaultAsync(user =>
            user.Id == userId && (user.Role == UserRole.Client || user.Role == UserRole.Admin));

        if (client == null)
        {
            throw new InvalidOperationException("Client profile not found");
        }

        client.FullName = request.FullName.Trim();
        client.Headline = request.Headline.Trim();
        client.AvatarUrl = request.Avatar.Trim();
        client.Location = request.Location.Trim();
        client.Category = request.CompanyName.Trim();
        client.ShortDescription = request.CompanyDescription.Trim();
        client.About = request.About.Trim();
        client.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Map(client);
    }

    private static ClientProfileDto Map(Domain.Entities.User user)
    {
        return new ClientProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Headline = user.Headline,
            Avatar = user.AvatarUrl,
            Location = user.Location,
            CompanyName = user.Category,
            CompanyDescription = user.ShortDescription,
            About = user.About,
        };
    }
}
