using Freelance.Application.DTOs.Projects;

namespace Freelance.Application.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponse> CreateAsync(Guid userId, CreateProjectRequest request);
        Task<IEnumerable<ProjectResponse>> GetAllAsync();
        Task<IEnumerable<ProjectResponse>> GetMineAsync(Guid userId);
        Task<ProjectResponse> UpdateAsync(Guid userId, Guid projectId, UpdateProjectRequest request);
        Task DeleteAsync(Guid userId, Guid projectId);
    }
}
