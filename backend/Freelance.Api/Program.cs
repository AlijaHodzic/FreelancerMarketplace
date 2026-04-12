using BCrypt.Net;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Freelance.Infrastructure.Data;
using Freelance.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var sqliteConnectionString = BuildSqliteConnectionString(builder.Configuration.GetConnectionString("Default")!, builder.Environment.ContentRootPath);
var allowedOrigins = GetAllowedOrigins(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(sqliteConnectionString);
});

builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientProfileService, ClientProfileService>();
builder.Services.AddScoped<IFavoriteFreelancerService, FavoriteFreelancerService>();
builder.Services.AddScoped<IFreelancerProfileService, FreelancerProfileService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.IncludeErrorDetails = true;
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var safeMessage = context.Exception.GetBaseException().Message
                .Replace("\r", " ")
                .Replace("\n", " ");
            context.Response.Headers.Append("X-Auth-Error", safeMessage);
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Freelance Marketplace API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await EnsureSqliteSchemaAsync(dbContext, sqliteConnectionString);
    await SeedDemoAdminAsync(dbContext);
    await SeedDemoFreelancersAsync(dbContext);
    await SeedDemoClientsAndProjectsAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static string[] GetAllowedOrigins(IConfiguration configuration)
{
    var configuredOrigins = configuration["Cors:AllowedOrigins"];
    if (!string.IsNullOrWhiteSpace(configuredOrigins))
    {
        return configuredOrigins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    return new[]
    {
        "http://localhost:4200",
        "https://localhost:4200",
    };
}

static string BuildSqliteConnectionString(string connectionString, string contentRootPath)
{
    var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);

    if (!Path.IsPathRooted(sqliteBuilder.DataSource))
    {
        sqliteBuilder.DataSource = Path.Combine(contentRootPath, sqliteBuilder.DataSource);
    }

    return sqliteBuilder.ToString();
}

static async Task EnsureSqliteSchemaAsync(AppDbContext dbContext, string connectionString)
{
    var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);
    var dataSource = sqliteBuilder.DataSource;

    if (!File.Exists(dataSource))
    {
        await dbContext.Database.EnsureCreatedAsync();
        return;
    }

    await dbContext.Database.OpenConnectionAsync();

    var ownerIdColumnExists = false;
    var conversationsTableExists = false;
    var slugColumnExists = false;
    var favoriteFreelancersTableExists = false;
    var assignedFreelancerColumnExists = false;
    var reviewsTableExists = false;
    var notificationsTableExists = false;

    try
    {
        await using DbCommand ownerCommand = dbContext.Database.GetDbConnection().CreateCommand();
        ownerCommand.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Projects') WHERE name = 'OwnerId';";
        ownerIdColumnExists = Convert.ToInt32(await ownerCommand.ExecuteScalarAsync()) > 0;

        await using DbCommand conversationsCommand = dbContext.Database.GetDbConnection().CreateCommand();
        conversationsCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Conversations';";
        conversationsTableExists = Convert.ToInt32(await conversationsCommand.ExecuteScalarAsync()) > 0;

        await using DbCommand slugColumnCommand = dbContext.Database.GetDbConnection().CreateCommand();
        slugColumnCommand.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Users') WHERE name = 'Slug';";
        slugColumnExists = Convert.ToInt32(await slugColumnCommand.ExecuteScalarAsync()) > 0;

        await using DbCommand favoritesTableCommand = dbContext.Database.GetDbConnection().CreateCommand();
        favoritesTableCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'FavoriteFreelancers';";
        favoriteFreelancersTableExists = Convert.ToInt32(await favoritesTableCommand.ExecuteScalarAsync()) > 0;

        await using DbCommand assignedFreelancerCommand = dbContext.Database.GetDbConnection().CreateCommand();
        assignedFreelancerCommand.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Projects') WHERE name = 'AssignedFreelancerId';";
        assignedFreelancerColumnExists = Convert.ToInt32(await assignedFreelancerCommand.ExecuteScalarAsync()) > 0;

        await using DbCommand reviewsTableCommand = dbContext.Database.GetDbConnection().CreateCommand();
        reviewsTableCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Reviews';";
        reviewsTableExists = Convert.ToInt32(await reviewsTableCommand.ExecuteScalarAsync()) > 0;

        await using DbCommand notificationsTableCommand = dbContext.Database.GetDbConnection().CreateCommand();
        notificationsTableCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Notifications';";
        notificationsTableExists = Convert.ToInt32(await notificationsTableCommand.ExecuteScalarAsync()) > 0;
    }
    finally
    {
        await dbContext.Database.CloseConnectionAsync();
    }

    if (ownerIdColumnExists && conversationsTableExists && slugColumnExists && favoriteFreelancersTableExists && assignedFreelancerColumnExists && reviewsTableExists && notificationsTableExists)
    {
        return;
    }

    var backupPath = Path.Combine(
        Path.GetDirectoryName(dataSource)!,
        $"{Path.GetFileNameWithoutExtension(dataSource)}.backup-{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(dataSource)}");

    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    var allowDestructiveReset = string.Equals(
        Environment.GetEnvironmentVariable("ALLOW_DESTRUCTIVE_DB_RESET"),
        "true",
        StringComparison.OrdinalIgnoreCase);

    if (!string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase) && !allowDestructiveReset)
    {
        throw new InvalidOperationException(
            $"Database schema is outdated for environment '{environment}'. " +
            $"Set ALLOW_DESTRUCTIVE_DB_RESET=true only if you explicitly want to recreate the SQLite demo database.");
    }

    File.Copy(dataSource, backupPath, overwrite: true);
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();
}

static async Task SeedDemoFreelancersAsync(AppDbContext dbContext)
{
    var demoFreelancers = new List<User>
    {
        new()
        {
            FullName = "Alex Rivera",
            Email = "alex.rivera@freelancehub.demo",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"),
            Role = UserRole.Freelancer,
            Slug = "alex-rivera",
            Headline = "Full Stack Developer",
            AvatarUrl = "https://i.pravatar.cc/160?img=12",
            Location = "San Francisco, CA",
            Category = "Web Development",
            ExperienceLevel = "Expert",
            ResponseTime = "1 hour",
            SuccessRate = "98%",
            CompletedProjects = 156,
            HourlyRate = 85m,
            Rating = 4.9m,
            ReviewsCount = 127,
            ShortDescription = "Builds scalable web platforms, polished client portals, and production-ready dashboard experiences.",
            About = "Full-stack developer with 8+ years of experience building scalable web applications. Specialized in modern JavaScript frameworks and cloud architecture. Passionate about clean code, performance, and user-centric product design.",
            SkillsJson = JsonSerializer.Serialize(new[] { "React", "Node.js", "TypeScript", "PostgreSQL", "AWS" }),
            PortfolioJson = JsonSerializer.Serialize(new[]
            {
                new { Title = "E-commerce Platform", Summary = "Built a complete e-commerce solution with React and Node.js, handling 10k+ daily transactions.", Image = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=80", Tags = new[] { "React", "Node.js", "Stripe" } },
                new { Title = "SaaS Dashboard", Summary = "Developed an analytics dashboard for a B2B SaaS company with real-time data visualization.", Image = "https://images.unsplash.com/photo-1551288049-bebda4e38f71?auto=format&fit=crop&w=900&q=80", Tags = new[] { "TypeScript", "D3.js", "PostgreSQL" } },
                new { Title = "Mobile Banking App", Summary = "Created a secure mobile banking application with biometric authentication and audit-ready flows.", Image = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=900&q=80", Tags = new[] { "React Native", "AWS", "Security" } },
            }),
            TestimonialsJson = JsonSerializer.Serialize(new[]
            {
                new { Author = "David Park", Project = "E-commerce Platform Development", Date = "Dec 15, 2025", Rating = 5, Comment = "Alex delivered exceptional work on our e-commerce platform. Great communication, technical expertise, and attention to detail. Highly recommend!", Avatar = "https://i.pravatar.cc/80?img=21" },
                new { Author = "Lisa Martinez", Project = "SaaS Dashboard Development", Date = "Nov 28, 2025", Rating = 5, Comment = "Outstanding developer! Alex understood our requirements perfectly and delivered ahead of schedule. Will definitely work together again.", Avatar = "https://i.pravatar.cc/80?img=31" },
                new { Author = "James Wilson", Project = "API Integration", Date = "Nov 10, 2025", Rating = 5, Comment = "Professional, skilled, and reliable. Alex went above and beyond to ensure our project was a success.", Avatar = "https://i.pravatar.cc/80?img=41" },
            }),
        },
        new() { FullName = "Amina Kovac", Email = "amina.kovac@freelancehub.demo", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"), Role = UserRole.Freelancer, Slug = "amina-kovac", Headline = "Senior Product Designer", AvatarUrl = "https://i.pravatar.cc/160?img=22", Location = "Sarajevo, BiH", Category = "Product Design", ExperienceLevel = "Senior", ResponseTime = "2 hours", SuccessRate = "97%", CompletedProjects = 84, HourlyRate = 32m, Rating = 4.9m, ReviewsCount = 94, ShortDescription = "Creates polished SaaS flows, mobile onboarding, and conversion-focused landing pages.", About = "Product designer focused on SaaS interfaces, onboarding systems, and clean visual storytelling. Loves turning unclear product ideas into intuitive journeys and design systems.", SkillsJson = JsonSerializer.Serialize(new[] { "Figma", "Design Systems", "UX Research" }), PortfolioJson = "[]", TestimonialsJson = "[]" },
        new() { FullName = "Luka Marin", Email = "luka.marin@freelancehub.demo", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"), Role = UserRole.Freelancer, Slug = "luka-marin", Headline = "Full-Stack Developer", AvatarUrl = "https://i.pravatar.cc/160?img=32", Location = "Split, HR", Category = "Web Development", ExperienceLevel = "Senior", ResponseTime = "3 hours", SuccessRate = "96%", CompletedProjects = 71, HourlyRate = 45m, Rating = 4.8m, ReviewsCount = 67, ShortDescription = "Builds production-ready dashboards, API integrations, and scalable marketplace features.", About = "Full-stack engineer experienced in Angular, Node.js, and backend-heavy business systems with a strong eye for maintainability.", SkillsJson = JsonSerializer.Serialize(new[] { "Angular", "Node.js", "PostgreSQL" }), PortfolioJson = "[]", TestimonialsJson = "[]" },
        new() { FullName = "Lejla Hadzic", Email = "lejla.hadzic@freelancehub.demo", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"), Role = UserRole.Freelancer, Slug = "lejla-hadzic", Headline = "Content Strategist", AvatarUrl = "https://i.pravatar.cc/160?img=42", Location = "Tuzla, BiH", Category = "Content", ExperienceLevel = "Expert", ResponseTime = "4 hours", SuccessRate = "95%", CompletedProjects = 133, HourlyRate = 24m, Rating = 4.7m, ReviewsCount = 156, ShortDescription = "Writes conversion copy and content systems tailored for startups and service businesses.", About = "Content strategist helping startups clarify their offer, improve acquisition funnels, and build consistent brand messaging.", SkillsJson = JsonSerializer.Serialize(new[] { "SEO", "Copywriting", "Content Plans" }), PortfolioJson = "[]", TestimonialsJson = "[]" },
        new() { FullName = "Mia Petrovic", Email = "mia.petrovic@freelancehub.demo", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"), Role = UserRole.Freelancer, Slug = "mia-petrovic", Headline = "Mobile App Developer", AvatarUrl = "https://i.pravatar.cc/160?img=52", Location = "Belgrade, RS", Category = "Mobile Development", ExperienceLevel = "Expert", ResponseTime = "1 hour", SuccessRate = "98%", CompletedProjects = 109, HourlyRate = 52m, Rating = 4.9m, ReviewsCount = 121, ShortDescription = "Helps startups launch reliable mobile products with strong UX and production-ready code.", About = "Mobile engineer focused on React Native product delivery, polished release cycles, and startup MVP acceleration.", SkillsJson = JsonSerializer.Serialize(new[] { "React Native", "iOS", "Android" }), PortfolioJson = "[]", TestimonialsJson = "[]" },
        new() { FullName = "Omar Selimovic", Email = "omar.selimovic@freelancehub.demo", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"), Role = UserRole.Freelancer, Slug = "omar-selimovic", Headline = "Data & Automation Specialist", AvatarUrl = "https://i.pravatar.cc/160?img=62", Location = "Mostar, BiH", Category = "Automation", ExperienceLevel = "Senior", ResponseTime = "2 hours", SuccessRate = "97%", CompletedProjects = 76, HourlyRate = 38m, Rating = 4.8m, ReviewsCount = 81, ShortDescription = "Builds internal tools, reporting flows, and automations that save teams hours each week.", About = "Automation-focused engineer working across Python, dashboards, and operations tooling for growing teams.", SkillsJson = JsonSerializer.Serialize(new[] { "Python", "Dashboards", "Automation" }), PortfolioJson = "[]", TestimonialsJson = "[]" },
        new() { FullName = "Nina Basic", Email = "nina.basic@freelancehub.demo", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"), Role = UserRole.Freelancer, Slug = "nina-basic", Headline = "Brand Designer", AvatarUrl = "https://i.pravatar.cc/160?img=72", Location = "Zagreb, HR", Category = "Brand Design", ExperienceLevel = "Senior", ResponseTime = "5 hours", SuccessRate = "99%", CompletedProjects = 98, HourlyRate = 29m, Rating = 4.9m, ReviewsCount = 112, ShortDescription = "Creates clean visual identities, marketing assets, and launch-ready social media packs.", About = "Brand designer helping service businesses and startups show up with stronger visual systems and clearer presentation.", SkillsJson = JsonSerializer.Serialize(new[] { "Branding", "Illustration", "Social Media" }), PortfolioJson = "[]", TestimonialsJson = "[]" },
    };

    foreach (var demoFreelancer in demoFreelancers)
    {
        var exists = await dbContext.Users.AnyAsync(user => user.Email == demoFreelancer.Email);
        if (exists)
        {
            continue;
        }

        dbContext.Users.Add(demoFreelancer);
    }

    await dbContext.SaveChangesAsync();
}

static async Task SeedDemoAdminAsync(AppDbContext dbContext)
{
    if (await dbContext.Users.AnyAsync(user => user.Email == "admin@freelancehub.demo"))
    {
        return;
    }

    dbContext.Users.Add(new User
    {
        FullName = "Admin User",
        Email = "admin@freelancehub.demo",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"),
        Role = UserRole.Admin,
        Slug = "admin-user",
        Headline = "Platform Administrator",
        AvatarUrl = "https://i.pravatar.cc/160?img=68",
        Location = "Remote",
        Category = "Platform Operations",
        ExperienceLevel = "Admin",
        ResponseTime = "Same day",
        SuccessRate = "100%",
        ShortDescription = "Demo admin account for reviewing platform activity.",
        About = "Demo administrator used to inspect marketplace users, projects, bids, messaging, and notification metrics.",
    });

    await dbContext.SaveChangesAsync();
}

static async Task SeedDemoClientsAndProjectsAsync(AppDbContext dbContext)
{
    var clientSeed = new[]
    {
        new { FullName = "Nova Commerce", Email = "hello@novacommerce.demo" },
        new { FullName = "Metric Labs", Email = "team@metriclabs.demo" },
    };

    foreach (var seed in clientSeed)
    {
        if (await dbContext.Users.AnyAsync(user => user.Email == seed.Email))
        {
            continue;
        }

        dbContext.Users.Add(new User
        {
            FullName = seed.FullName,
            Email = seed.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234"),
            Role = UserRole.Client,
            Slug = seed.FullName.Trim().ToLowerInvariant().Replace(" ", "-"),
            Headline = "Client",
            AvatarUrl = "https://i.pravatar.cc/160?img=5",
            Location = "Remote",
            Category = "Business",
            ExperienceLevel = "Established",
            ResponseTime = "Same day",
            SuccessRate = "100%",
            ShortDescription = "Posts funded projects and collaborates with vetted freelancers.",
            About = "Demo client account used to seed realistic marketplace data.",
        });
    }

    await dbContext.SaveChangesAsync();

    if (await dbContext.Projects.AnyAsync())
    {
        return;
    }

    var novaCommerce = await dbContext.Users.FirstAsync(user => user.Email == "hello@novacommerce.demo");
    var metricLabs = await dbContext.Users.FirstAsync(user => user.Email == "team@metriclabs.demo");
    var alex = await dbContext.Users.FirstAsync(user => user.Email == "alex.rivera@freelancehub.demo");
    var amina = await dbContext.Users.FirstAsync(user => user.Email == "amina.kovac@freelancehub.demo");
    var luka = await dbContext.Users.FirstAsync(user => user.Email == "luka.marin@freelancehub.demo");

    var projects = new[]
    {
        new Project
        {
            OwnerId = novaCommerce.Id,
            Title = "Homepage refresh and conversion cleanup",
            Description = "We need a sharper homepage, faster landing experience, and cleaner trial signup flow for our ecommerce SaaS.",
            BudgetMin = 1800,
            BudgetMax = 3200,
            Status = ProjectStatus.Open,
        },
        new Project
        {
            OwnerId = metricLabs.Id,
            Title = "Analytics dashboard revamp",
            Description = "Redesign and rebuild our reporting dashboard with clearer charts, filtered views, and better client export flows.",
            BudgetMin = 2500,
            BudgetMax = 4800,
            Status = ProjectStatus.Open,
        },
        new Project
        {
            OwnerId = novaCommerce.Id,
            Title = "Client portal integration",
            Description = "Build an authenticated portal area that lets clients review deliverables, invoices, and progress milestones.",
            BudgetMin = 1400,
            BudgetMax = 2600,
            Status = ProjectStatus.Open,
        },
    };

    dbContext.Projects.AddRange(projects);
    await dbContext.SaveChangesAsync();

    dbContext.Bids.AddRange(
        new Bid
        {
            ProjectId = projects[0].Id,
            FreelancerId = alex.Id,
            Amount = 2900,
            Message = "I can redesign the homepage and improve the signup funnel with a faster React implementation, analytics instrumentation, and clear testing checkpoints.",
            Status = BidStatus.Pending,
        },
        new Bid
        {
            ProjectId = projects[1].Id,
            FreelancerId = amina.Id,
            Amount = 3400,
            Message = "I would approach this as a product design and UX cleanup project first, then hand off polished flows and component specs for implementation.",
            Status = BidStatus.Pending,
        },
        new Bid
        {
            ProjectId = projects[2].Id,
            FreelancerId = luka.Id,
            Amount = 2200,
            Message = "I can deliver a maintainable Angular-based portal with secure API integration, milestone tracking, and a client-friendly information architecture.",
            Status = BidStatus.Pending,
        });

    await dbContext.SaveChangesAsync();
}
