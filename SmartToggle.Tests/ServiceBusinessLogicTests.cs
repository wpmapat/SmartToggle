using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SmartToggle.BusinessLogic;
using SmartToggle.Models;
using Xunit;

namespace SmartToggle.Tests
{
    public class ServiceBusinessLogicTests
    {
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly Mock<ICompanyRepository> _companyRepo = new();
        private readonly Mock<IFeatureFlagRepository> _featureFlagRepo = new();
        private readonly ServiceBusinessLogic _sut;

        public ServiceBusinessLogicTests()
        {
            _sut = new ServiceBusinessLogic(_serviceRepo.Object, _companyRepo.Object, _featureFlagRepo.Object);
        }

        [Fact]
        public async Task DeleteServiceAsync_WhenFeatureFlagsExist_ThrowsInvalidOperationException()
        {
            var serviceId = "service-1";
            var service = new Service { Id = serviceId, CompanyId = "company-1" };
            var flags = new List<FeatureFlag<bool>> { new FeatureFlag<bool> { Id = "flag-1" } };

            _serviceRepo.Setup(r => r.GetByIdAsync(serviceId)).ReturnsAsync(service);
            _featureFlagRepo.Setup(r => r.GetByServiceIdAsync(serviceId)).ReturnsAsync(flags);

            await Assert.ThrowsAsync<System.InvalidOperationException>(
                () => _sut.DeleteServiceAsync(serviceId));
        }

        [Fact]
        public async Task DeleteServiceAsync_WhenServiceNotFound_ReturnsFalse()
        {
            _serviceRepo.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((Service?)null);

            var result = await _sut.DeleteServiceAsync("missing");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteServiceAsync_WhenNoFeatureFlags_DeletesSuccessfully()
        {
            var serviceId = "service-1";
            var service = new Service { Id = serviceId, CompanyId = "company-1" };

            _serviceRepo.Setup(r => r.GetByIdAsync(serviceId)).ReturnsAsync(service);
            _featureFlagRepo.Setup(r => r.GetByServiceIdAsync(serviceId)).ReturnsAsync(new List<FeatureFlag<bool>>());
            _serviceRepo.Setup(r => r.DeleteAsync(serviceId, service.CompanyId)).ReturnsAsync(true);

            var result = await _sut.DeleteServiceAsync(serviceId);

            Assert.True(result);
        }

        [Fact]
        public async Task CreateServiceAsync_WhenCompanyNotFound_ThrowsException()
        {
            var service = new Service { ServiceName = "My Service", CompanyId = "missing-company" };

            _companyRepo.Setup(r => r.GetByIdAsync("missing-company")).ReturnsAsync((Company?)null);

            await Assert.ThrowsAsync<System.Exception>(
                () => _sut.CreateServiceAsync(service));
        }

        [Fact]
        public async Task CreateServiceAsync_WhenCompanyExists_ReturnsCreatedService()
        {
            var company = new Company { Id = "company-1" };
            var service = new Service { ServiceName = "My Service", CompanyId = "company-1", Description = "desc" };
            var created = new Service { Id = "new-id", ServiceName = "My Service", CompanyId = "company-1" };

            _companyRepo.Setup(r => r.GetByIdAsync("company-1")).ReturnsAsync(company);
            _serviceRepo.Setup(r => r.AddAsync(It.IsAny<Service>())).ReturnsAsync(created);

            var result = await _sut.CreateServiceAsync(service);

            Assert.NotNull(result);
            Assert.Equal("new-id", result.Id);
        }
    }
}
