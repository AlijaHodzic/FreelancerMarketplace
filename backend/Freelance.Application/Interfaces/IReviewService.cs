using Freelance.Application.DTOs.Reviews;

namespace Freelance.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(Guid clientId, CreateReviewRequest request);
    Task<IEnumerable<ReviewDto>> GetForFreelancerAsync(Guid freelancerId);
}
