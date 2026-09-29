using FluentAssertions;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Implementations;
using ServiceRequestManagementSystem.Tests.Helpers;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Repositories
{
    public class GenericRepositoryTests
    {
        [Fact]
        public async Task AddAsync_ShouldAddEntityToDatabase()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var repo = new GenericRepository<Department>(context);
            var department = new Department
            {
                DepartmentName = "Engineering",
                DepartmentCode = "ENG",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            await repo.AddAsync(department);
            await context.SaveChangesAsync();

            // Assert
            var result = await repo.GetByIdAsync(department.DepartmentId);
            result.Should().NotBeNull();
            result!.DepartmentName.Should().Be("Engineering");
            result.DepartmentCode.Should().Be("ENG");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEntities()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var repo = new GenericRepository<Department>(context);
            await repo.AddRangeAsync(new[]
            {
                new Department { DepartmentName = "HR", DepartmentCode = "HR", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Department { DepartmentName = "Finance", DepartmentCode = "FIN", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });
            await context.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync();

            // Assert
            all.Should().HaveCount(2);
        }

        [Fact]
        public async Task FindAsync_WithPredicate_ShouldReturnMatchingEntities()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var repo = new GenericRepository<Department>(context);
            await repo.AddRangeAsync(new[]
            {
                new Department { DepartmentName = "Support Active", DepartmentCode = "SA", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Department { DepartmentName = "Support Inactive", DepartmentCode = "SI", IsActive = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });
            await context.SaveChangesAsync();

            // Act
            var activeOnly = await repo.FindAsync(d => d.IsActive);

            // Assert
            activeOnly.Should().ContainSingle();
            activeOnly.First().DepartmentCode.Should().Be("SA");
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrueIfExistsAndFalseIfNot()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var repo = new GenericRepository<Department>(context);
            await repo.AddAsync(new Department
            {
                DepartmentName = "Sales",
                DepartmentCode = "SALES",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            // Act & Assert
            var exists = await repo.ExistsAsync(d => d.DepartmentCode == "SALES");
            var notExists = await repo.ExistsAsync(d => d.DepartmentCode == "UNKNOWN");

            exists.Should().BeTrue();
            notExists.Should().BeFalse();
        }

        [Fact]
        public async Task CountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var repo = new GenericRepository<Department>(context);
            await repo.AddRangeAsync(new[]
            {
                new Department { DepartmentName = "D1", DepartmentCode = "D1", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Department { DepartmentName = "D2", DepartmentCode = "D2", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountAsync();

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public async Task Remove_ShouldDeleteEntity()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var repo = new GenericRepository<Department>(context);
            var dept = new Department
            {
                DepartmentName = "Temp",
                DepartmentCode = "TMP",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await repo.AddAsync(dept);
            await context.SaveChangesAsync();

            // Act
            repo.Remove(dept);
            await context.SaveChangesAsync();

            // Assert
            var deleted = await repo.GetByIdAsync(dept.DepartmentId);
            deleted.Should().BeNull();
        }
    }
}
