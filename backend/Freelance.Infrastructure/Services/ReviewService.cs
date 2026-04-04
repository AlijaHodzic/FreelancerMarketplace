using Freelance.Application.DTOs.Reviews;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Freelance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public ReviewService(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<ReviewDto> CreateAsync(Guid clientId, CreateReviewRequest request)
    {
        if (request.Rating is < 1 or > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5");
        }

        var project = await _db.Projects
            .Include(item => item.Owner)
            .Include(item => item.AssignedFreelancer)
            .Include(item => item.Reviews)
            .FirstOrDefaultAsync(item => item.Id == request.ProjectId);

        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        if (project.OwnerId != clientId)
        {
            throw new InvalidOperationException("Only the project owner can leave a review");
        }

        if (project.Status != ProjectStatus.Completed || project.AssignedFreelancerId == null)
        {
            throw new InvalidOperationException("Reviews are available only for completed projects with a hired freelancer");
        }

        if (project.Reviews.Any())
        {
            throw new InvalidOperationException("A review already exists for this project");
        }

        var review = new Review
        {
            ProjectId = project.Id,
            ClientId = clientId,
            FreelancerId = project.AssignedFreelancerId.Value,
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var freelancer = project.AssignedFreelancer!;
        freelancer.ReviewsCount += 1;
        freelancer.Rating = await _db.Reviews
            .Where(item => item.FreelancerId == freelancer.Id)
            .Select(item => (decimal)item.Rating)
            .DefaultIfEmpty(freelancer.Rating)
            .AverageAsync();

        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(
            freelancer.Id,
            "review-received",
            "New client review",
            $"{project.Owner.FullName} left you a {request.Rating}-star review for \"{project.Title}\".",
            $"/freelancers/{freelancer.Slug}");

        return Map(review, project.Title, project.Owner.FullName, freelancer.FullName);
    }

    public async Task<IEnumerable<ReviewDto>> GetForFreelancerAsync(Guid freelancerId)
    {
        var reviews = await _db.Reviews
            .Include(item => item.Client)
            .Include(item => item.Freelancer)
            .Include(item => item.Project)
            .Where(item => item.FreelancerId == freelancerId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .ToListAsync();

        return reviews.Select(review => Map(review, review.Project.Title, review.Client.FullName, review.Freelancer.FullName));
    }

    private static ReviewDto Map(Review review, string projectTitle, string clientName, string freelancerName)
    {
        return new ReviewDto
        {
            Id = review.Id,
            ProjectId = review.ProjectId,
            ClientId = review.ClientId,
            FreelancerId = review.FreelancerId,
            ClientName = clientName,
            FreelancerName = freelancerName,
            ProjectTitle = projectTitle,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAtUtc = review.CreatedAtUtc,
        };
    }
}
