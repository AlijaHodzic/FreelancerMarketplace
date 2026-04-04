using System.Text.Json;
using Freelance.Application.DTOs.Freelancers;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Freelance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Services;

public class FreelancerProfileService : IFreelancerProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AppDbContext _db;

    public FreelancerProfileService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<FreelancerSummaryDto>> GetAllAsync()
    {
        var freelancers = await _db.Users
            .Where(user => user.Role == UserRole.Freelancer)
            .ToListAsync();

        return freelancers
            .OrderByDescending(user => user.Rating)
            .ThenByDescending(user => user.CompletedProjects)
            .Select(MapSummary);
    }

    public async Task<FreelancerProfileDto> GetBySlugAsync(string slug)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var freelancer = await _db.Users
            .FirstOrDefaultAsync(user => user.Role == UserRole.Freelancer && user.Slug == normalizedSlug);

        if (freelancer == null)
        {
            throw new InvalidOperationException("Freelancer not found");
        }

        return MapProfile(freelancer);
    }

    public async Task<FreelancerProfileDto> GetMineAsync(Guid userId)
    {
        var freelancer = await _db.Users.FirstOrDefaultAsync(user => user.Id == userId && user.Role == UserRole.Freelancer);
        if (freelancer == null)
        {
            throw new InvalidOperationException("Freelancer profile not found");
        }

        return MapProfile(freelancer);
    }

    public async Task<FreelancerProfileDto> UpdateAsync(Guid userId, UpdateFreelancerProfileRequest request)
    {
        var freelancer = await _db.Users.FirstOrDefaultAsync(user => user.Id == userId && user.Role == UserRole.Freelancer);
        if (freelancer == null)
        {
            throw new InvalidOperationException("Freelancer profile not found");
        }

        freelancer.FullName = request.FullName.Trim();
        freelancer.Slug = await BuildUniqueSlugAsync(request.FullName, freelancer.Id);
        freelancer.Headline = request.Headline.Trim();
        freelancer.AvatarUrl = request.Avatar.Trim();
        freelancer.Location = request.Location.Trim();
        freelancer.Category = request.Category.Trim();
        freelancer.ExperienceLevel = request.Experience.Trim();
        freelancer.ResponseTime = request.ResponseTime.Trim();
        freelancer.SuccessRate = request.SuccessRate.Trim();
        freelancer.CompletedProjects = Math.Max(request.CompletedProjects, 0);
        freelancer.HourlyRate = Math.Max(request.HourlyRate, 1);
        freelancer.ShortDescription = request.Description.Trim();
        freelancer.About = request.About.Trim();
        freelancer.SkillsJson = JsonSerializer.Serialize(
            request.Skills
                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                .Select(skill => skill.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList());
        freelancer.PortfolioJson = JsonSerializer.Serialize(request.Portfolio.Select(item => new FreelancerPortfolioItemDto
        {
            Title = item.Title.Trim(),
            Summary = item.Summary.Trim(),
            Image = item.Image.Trim(),
            Tags = item.Tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
        }).Where(item => !string.IsNullOrWhiteSpace(item.Title) || !string.IsNullOrWhiteSpace(item.Summary)).ToList());
        freelancer.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapProfile(freelancer);
    }

    public static FreelancerSummaryDto MapSummary(User user)
    {
        return new FreelancerSummaryDto
        {
            Id = user.Id,
            Slug = user.Slug,
            Email = user.Email,
            Avatar = user.AvatarUrl,
            Name = user.FullName,
            Title = user.Headline,
            Rating = user.Rating,
            Reviews = user.ReviewsCount,
            HourlyRate = user.HourlyRate,
            Location = user.Location,
            Category = user.Category,
            Experience = user.ExperienceLevel,
            ResponseTime = user.ResponseTime,
            SuccessRate = user.SuccessRate,
            CompletedProjects = user.CompletedProjects,
            Description = user.ShortDescription,
            Skills = Deserialize<List<string>>(user.SkillsJson) ?? new List<string>(),
        };
    }

    private static FreelancerProfileDto MapProfile(User user)
    {
        var summary = MapSummary(user);

        return new FreelancerProfileDto
        {
            Id = summary.Id,
            Slug = summary.Slug,
            Email = summary.Email,
            Avatar = summary.Avatar,
            Name = summary.Name,
            Title = summary.Title,
            Rating = summary.Rating,
            Reviews = summary.Reviews,
            HourlyRate = summary.HourlyRate,
            Location = summary.Location,
            Category = summary.Category,
            Experience = summary.Experience,
            ResponseTime = summary.ResponseTime,
            SuccessRate = summary.SuccessRate,
            CompletedProjects = summary.CompletedProjects,
            Description = summary.Description,
            Skills = summary.Skills,
            About = user.About,
            Portfolio = Deserialize<List<FreelancerPortfolioItemDto>>(user.PortfolioJson) ?? new List<FreelancerPortfolioItemDto>(),
            Testimonials = Deserialize<List<FreelancerReviewDto>>(user.TestimonialsJson) ?? new List<FreelancerReviewDto>(),
        };
    }

    private static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    private async Task<string> BuildUniqueSlugAsync(string fullName, Guid currentUserId)
    {
        var baseSlug = string.Join(
            "-",
            fullName
                .Trim()
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = $"freelancer-{currentUserId.ToString()[..8]}";
        }

        var slug = baseSlug;
        var suffix = 2;

        while (await _db.Users.AnyAsync(user => user.Id != currentUserId && user.Slug == slug))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix += 1;
        }

        return slug;
    }
}
