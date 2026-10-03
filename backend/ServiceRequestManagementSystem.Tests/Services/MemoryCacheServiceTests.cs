using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using ServiceRequestManagementSystem.API.Services.Implementations;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Services
{
    public class MemoryCacheServiceTests
    {
        [Fact]
        public async Task GetOrCreateAsync_ShouldReturnCachedValueOnSecondCall()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var logger = NullLogger<MemoryCacheService>.Instance;
            var cacheService = new MemoryCacheService(memoryCache, logger);

            int callCount = 0;
            Func<Task<string>> factory = () =>
            {
                callCount++;
                return Task.FromResult($"Value-{callCount}");
            };

            // Act
            var first = await cacheService.GetOrCreateAsync("test_key", factory, TimeSpan.FromMinutes(5));
            var second = await cacheService.GetOrCreateAsync("test_key", factory, TimeSpan.FromMinutes(5));

            // Assert
            first.Should().Be("Value-1");
            second.Should().Be("Value-1");
            callCount.Should().Be(1);
        }

        [Fact]
        public async Task Remove_ShouldEvictKeyFromCache()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var logger = NullLogger<MemoryCacheService>.Instance;
            var cacheService = new MemoryCacheService(memoryCache, logger);

            await cacheService.SetAsync("remove_key", "StoredValue");

            // Act
            var before = await cacheService.GetAsync<string>("remove_key");
            cacheService.Remove("remove_key");
            var after = await cacheService.GetAsync<string>("remove_key");

            // Assert
            before.Should().Be("StoredValue");
            after.Should().BeNull();
        }

        [Fact]
        public async Task RemoveByPrefix_ShouldEvictAllMatchingKeys()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var logger = NullLogger<MemoryCacheService>.Instance;
            var cacheService = new MemoryCacheService(memoryCache, logger);

            await cacheService.SetAsync("prefix_1", "Value1");
            await cacheService.SetAsync("prefix_2", "Value2");
            await cacheService.SetAsync("other_key", "Value3");

            // Act
            cacheService.RemoveByPrefix("prefix_");

            var val1 = await cacheService.GetAsync<string>("prefix_1");
            var val2 = await cacheService.GetAsync<string>("prefix_2");
            var val3 = await cacheService.GetAsync<string>("other_key");

            // Assert
            val1.Should().BeNull();
            val2.Should().BeNull();
            val3.Should().Be("Value3");
        }
    }
}
