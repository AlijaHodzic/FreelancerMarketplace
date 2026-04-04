using Freelance.Application.DTOs.Freelancers;

namespace Freelance.Application.Interfaces;

public interface IFreelancerProfileService
{
    Task<IEnumerable<FreelancerSummaryDto>> GetAllAsync();
    Task<FreelancerProfileDto> GetBySlugAsync(string slug);
    Task<FreelancerProfileDto> GetMineAsync(Guid userId);
    Task<FreelancerProfileDto> UpdateAsync(Guid userId, UpdateFreelancerProfileRequest request);
}
