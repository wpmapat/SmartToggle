using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SmartToggle.BusinessLogic;
using SmartToggle.Models;
using Xunit;

namespace SmartToggle.Tests
{
    public class FeatureFlagBusinessLogicTests
    {
        private readonly Mock<IFeatureFlagRepository> _featureFlagRepo = new();
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly FeatureFlagBusinessLogic _sut;

        public FeatureFlagBusinessLogicTests()
        {
            _sut = new FeatureFlagBusinessLogic(_featureFlagRepo.Object, _serviceRepo.Object);
        }

        [Fact]
        public async Task CreateFeatureFlagAsync_WhenServiceNotFound_ThrowsException()
        {
            var flag = new FeatureFlag<bool>
            {
                FlagId = "flag-1",
                CompanyId = "company-1",
                ServiceId = "missing-service",
                Type = "bool",
                DefaultValue = true
            };

            _serviceRepo.Setup(r => r.GetByIdAsync("missing-service")).ReturnsAsync((Service?)null);

            await Assert.ThrowsAsync<System.Exception>(
                () => _sut.CreateFeatureFlagAsync(flag));
        }

        [Fact]
        public async Task CreateFeatureFlagAsync_WhenServiceExists_ReturnsCreatedFlag()
        {
            var service = new Service { Id = "service-1" };
            var flag = new FeatureFlag<bool>
            {
                FlagId = "flag-1",
                CompanyId = "company-1",
                ServiceId = "service-1",
                Type = "bool",
                DefaultValue = true
            };
            var created = new FeatureFlag<bool> { Id = "new-id", FlagId = "flag-1" };

            _serviceRepo.Setup(r => r.GetByIdAsync("service-1")).ReturnsAsync(service);
            _featureFlagRepo.Setup(r => r.AddAsync(It.IsAny<FeatureFlag<bool>>())).ReturnsAsync(created);

            var result = await _sut.CreateFeatureFlagAsync(flag);

            Assert.NotNull(result);
            Assert.Equal("new-id", result.Id);
        }

        [Fact]
        public async Task DeleteFeatureFlagAsync_WhenFlagNotFound_ReturnsFalse()
        {
            _featureFlagRepo.Setup(r => r.DeleteAsync("missing", "service-1")).ReturnsAsync(false);

            var result = await _sut.DeleteFeatureFlagAsync("missing", "service-1");

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateFeatureFlagAsync_WhenFlagNotFound_ReturnsNull()
        {
            _featureFlagRepo.Setup(r => r.GetByIdAsync("missing", "service-1")).ReturnsAsync((FeatureFlag<bool>?)null);

            var result = await _sut.UpdateFeatureFlagAsync("missing", new FeatureFlag<bool> { ServiceId = "service-1" });

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateFeatureFlagAsync_WhenFlagIdMissing_ThrowsArgumentException()
        {
            var flag = new FeatureFlag<bool>
            {
                FlagId = "",
                CompanyId = "company-1",
                ServiceId = "service-1",
                Type = "bool",
                DefaultValue = true
            };

            await Assert.ThrowsAsync<System.Exception>(
                () => _sut.CreateFeatureFlagAsync(flag));
        }
    }
}
