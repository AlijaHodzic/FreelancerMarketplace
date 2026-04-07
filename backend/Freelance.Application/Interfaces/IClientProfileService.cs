using Freelance.Application.DTOs.Clients;

namespace Freelance.Application.Interfaces;

public interface IClientProfileService
{
    Task<ClientProfileDto> GetMineAsync(Guid userId);
    Task<ClientProfileDto> UpdateAsync(Guid userId, UpdateClientProfileRequest request);
}
