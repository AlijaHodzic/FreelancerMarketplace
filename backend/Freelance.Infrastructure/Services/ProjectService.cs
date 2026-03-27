using Freelance.Application.DTOs.Projects;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Freelance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _db;

        public ProjectService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ProjectResponse> CreateAsync(Guid userId, CreateProjectRequest request)
        {
            var project = new Project
            {
                OwnerId = userId,
                Title = request.Title,
                Description = request.Description,
                BudgetMin = request.BudgetMin,
                BudgetMax = request.BudgetMax,
                Status = ProjectStatus.Open
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            return Map(project);
        }

        public async Task<IEnumerable<ProjectResponse>> GetAllAsync()
        {
            var projects = await _db.Projects
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            return projects.Select(Map);
        }

        public async Task<IEnumerable<ProjectResponse>> GetMineAsync(Guid userId)
        {
            var projects = await _db.Projects
                .Where(x => x.OwnerId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            return projects.Select(Map);
        }

        public async Task<ProjectResponse> UpdateAsync(Guid userId, Guid projectId, UpdateProjectRequest request)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(x => x.Id == projectId);

            if (project == null)
                throw new Exception("Project not found");

            if (project.OwnerId != userId)
                throw new Exception("Not allowed");

            project.Title = request.Title;
            project.Description = request.Description;
            project.BudgetMin = request.BudgetMin;
            project.BudgetMax = request.BudgetMax;
            project.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Map(project);
        }

        public async Task DeleteAsync(Guid userId, Guid projectId)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(x => x.Id == projectId);

            if (project == null)
                throw new Exception("Project not found");

            if (project.OwnerId != userId)
                throw new Exception("Not allowed");

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
        }

        private static ProjectResponse Map(Project project)
        {
            return new ProjectResponse
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                BudgetMin = project.BudgetMin,
                BudgetMax = project.BudgetMax,
                Status = project.Status,
                CreatedAtUtc = project.CreatedAtUtc
            };
        }
    }
}
