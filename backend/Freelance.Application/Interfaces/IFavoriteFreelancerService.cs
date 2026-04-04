using Freelance.Application.DTOs.Freelancers;

namespace Freelance.Application.Interfaces;

public interface IFavoriteFreelancerService
{
    Task<IEnumerable<FreelancerSummaryDto>> GetMineAsync(Guid clientId);
    Task AddAsync(Guid clientId, Guid freelancerId);
    Task RemoveAsync(Guid clientId, Guid freelancerId);
}
