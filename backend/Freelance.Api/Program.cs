using Freelance.Application.Interfaces;
using Freelance.Infrastructure.Data;
using Freelance.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var sqliteConnectionString = BuildSqliteConnectionString(builder.Configuration.GetConnectionString("Default")!, builder.Environment.ContentRootPath);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(sqliteConnectionString);
});

builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
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

    try
    {
        await using DbCommand command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Projects') WHERE name = 'OwnerId';";
        var ownerIdColumnExists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

        if (ownerIdColumnExists)
        {
            return;
        }
    }
    finally
    {
        await dbContext.Database.CloseConnectionAsync();
    }

    var backupPath = Path.Combine(
        Path.GetDirectoryName(dataSource)!,
        $"{Path.GetFileNameWithoutExtension(dataSource)}.backup-{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(dataSource)}");

    File.Copy(dataSource, backupPath, overwrite: true);
    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();
}
