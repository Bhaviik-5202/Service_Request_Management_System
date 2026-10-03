using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.Middleware;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Implementations;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;
using ServiceRequestManagementSystem.API.Services;
using ServiceRequestManagementSystem.API.Services.Implementations;
using ServiceRequestManagementSystem.API.Services.Interfaces;
using ServiceRequestManagementSystem.API.Validator;

namespace ServiceRequestManagementSystem.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Logging (Clear EventLog to prevent Win32Exception 8 on Windows)
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Controllers
            builder.Services.AddControllers();

            // CORS Configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // In-Memory Caching
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

            // Password Hasher & Token Services
            builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

            // JWT Settings & Token Service
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("Jwt"));
            builder.Services.AddSingleton<TokenService>();

            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

            // JWT Bearer Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings?.Issuer,
                        ValidAudience = jwtSettings?.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings?.Key ?? string.Empty))
                    };
                });

            // Authorization
            builder.Services.AddAuthorization();

            // Entity Framework Core + SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repository & Unit of Work Layer
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Business Service Layer
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
            builder.Services.AddScoped<IMasterService, MasterService>();
            builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
            builder.Services.AddScoped<IApprovalService, ApprovalService>();
            builder.Services.AddScoped<IAssetService, AssetService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IAuditLogService, AuditLogService>();

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

            // OpenAPI + JWT Bearer Authentication
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            });

            // Health Checks
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            // Custom Middlewares (Logging & Global Error Handling)
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Enable CORS before auth
            app.UseCors("AllowFrontend");

            // Scalar API Documentation
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference("/");
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Health Check Endpoint (accessible without auth for orchestrator/docker probes)
            app.MapHealthChecks("/health");

            // Authentication & Authorization Middlewares
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }

    // OpenAPI JWT Bearer Security Scheme Transformer
    internal sealed class BearerSecuritySchemeTransformer(
        IAuthenticationSchemeProvider authenticationSchemeProvider)
        : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var authenticationSchemes =
                await authenticationSchemeProvider.GetAllSchemesAsync();

            if (authenticationSchemes.Any(
                scheme => scheme.Name == JwtBearerDefaults.AuthenticationScheme))
            {
                document.Components ??= new OpenApiComponents();

                document.Components.SecuritySchemes =
                    new Dictionary<string, IOpenApiSecurityScheme>
                    {
                        ["Bearer"] = new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                            In = ParameterLocation.Header,
                            Name = "Authorization",
                            Description = "Enter JWT Bearer token"
                        }
                    };
            }
        }
    }
}
