using Freelance.Application.DTOs.Bids;

namespace Freelance.Application.Interfaces;

public interface IBidService
{
    Task<BidResponse> CreateAsync(Guid freelancerId, CreateBidRequest request);
    Task<IEnumerable<BidResponse>> GetByProjectAsync(Guid projectId, Guid requesterId);
    Task<IEnumerable<BidResponse>> GetMineAsync(Guid freelancerId);
    Task AcceptAsync(Guid bidId, Guid ownerId);
    Task RejectAsync(Guid bidId, Guid ownerId);
}
