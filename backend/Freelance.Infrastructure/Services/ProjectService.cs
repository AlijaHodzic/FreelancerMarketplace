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
        private readonly INotificationService _notificationService;

        public ProjectService(AppDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
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

            return await MapAsync(project, null);
        }

        public async Task<ProjectResponse> HireFreelancerAsync(Guid userId, HireFreelancerRequest request)
        {
            var freelancer = await _db.Users.FirstOrDefaultAsync(user => user.Id == request.FreelancerId && user.Role == UserRole.Freelancer);
            if (freelancer == null)
            {
                throw new InvalidOperationException("Freelancer not found");
            }

            var project = new Project
            {
                OwnerId = userId,
                AssignedFreelancerId = freelancer.Id,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                BudgetMin = request.BudgetMin,
                BudgetMax = request.BudgetMax,
                Status = ProjectStatus.InProgress,
                HiredAtUtc = DateTime.UtcNow,
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            project = await _db.Projects
                .Include(item => item.AssignedFreelancer)
                .FirstAsync(item => item.Id == project.Id);

            await _notificationService.CreateAsync(
                freelancer.Id,
                "direct-hire",
                "You were hired directly",
                $"A client started a direct engagement with you: \"{project.Title}\".",
                "/freelancer-dashboard");

            return await MapAsync(project, userId);
        }

        public async Task<IEnumerable<ProjectResponse>> GetAllAsync()
        {
            var projects = await _db.Projects
                .Include(item => item.AssignedFreelancer)
                .Include(item => item.Reviews)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            return projects.Select(project => Map(project, null));
        }

        public async Task<IEnumerable<ProjectResponse>> GetMineAsync(Guid userId)
        {
            var projects = await _db.Projects
                .Include(item => item.AssignedFreelancer)
                .Include(item => item.Reviews)
                .Where(x => x.OwnerId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            return projects.Select(project => Map(project, userId));
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

            return await MapAsync(project, userId);
        }

        public async Task<ProjectResponse> CompleteAsync(Guid userId, Guid projectId)
        {
            var project = await _db.Projects
                .Include(item => item.AssignedFreelancer)
                .Include(item => item.Reviews)
                .FirstOrDefaultAsync(item => item.Id == projectId);

            if (project == null)
            {
                throw new InvalidOperationException("Project not found");
            }

            if (project.OwnerId != userId)
            {
                throw new InvalidOperationException("Not allowed");
            }

            if (project.AssignedFreelancerId == null)
            {
                throw new InvalidOperationException("You need a hired freelancer before completing the project");
            }

            project.Status = ProjectStatus.Completed;
            project.CompletedAtUtc = DateTime.UtcNow;
            project.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _notificationService.CreateAsync(
                project.AssignedFreelancerId.Value,
                "project-completed",
                "Project marked completed",
                $"\"{project.Title}\" has been marked as completed. You may receive a review next.",
                "/freelancer-dashboard");

            return Map(project, userId);
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

        private async Task<ProjectResponse> MapAsync(Project project, Guid? currentUserId)
        {
            if (project.AssignedFreelancer == null && project.AssignedFreelancerId != null)
            {
                project.AssignedFreelancer = await _db.Users.FirstOrDefaultAsync(user => user.Id == project.AssignedFreelancerId);
            }

            if (project.Reviews.Count == 0)
            {
                project.Reviews = await _db.Reviews.Where(item => item.ProjectId == project.Id).ToListAsync();
            }

            return Map(project, currentUserId);
        }

        private static ProjectResponse Map(Project project, Guid? currentUserId)
        {
            return new ProjectResponse
            {
                Id = project.Id,
                OwnerId = project.OwnerId,
                Title = project.Title,
                Description = project.Description,
                BudgetMin = project.BudgetMin,
                BudgetMax = project.BudgetMax,
                Status = project.Status,
                AssignedFreelancerId = project.AssignedFreelancerId,
                AssignedFreelancerName = project.AssignedFreelancer?.FullName ?? string.Empty,
                AssignedFreelancerEmail = project.AssignedFreelancer?.Email ?? string.Empty,
                HiredAtUtc = project.HiredAtUtc,
                CompletedAtUtc = project.CompletedAtUtc,
                HasReview = project.Reviews.Any(),
                CanReview = currentUserId == project.OwnerId && project.Status == ProjectStatus.Completed && project.AssignedFreelancerId != null && !project.Reviews.Any(),
                CreatedAtUtc = project.CreatedAtUtc
            };
        }
    }
}
