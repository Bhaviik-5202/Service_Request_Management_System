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
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Services;
using ServiceRequestManagementSystem.API.Validator;

namespace ServiceRequestManagementSystem.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controllers
            builder.Services.AddControllers();

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

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

            // OpenAPI + JWT Bearer Authentication
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            });

            var app = builder.Build();

            // Scalar
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference("/");
            }

            app.UseHttpsRedirection();

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
