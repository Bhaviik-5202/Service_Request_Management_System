using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ServiceRequestManagementSystem.API.Data;
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

            // Entity Framework Core + SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Scalar
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference("/");
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}
