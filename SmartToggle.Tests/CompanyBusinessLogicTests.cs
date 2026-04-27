using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SmartToggle.BusinessLogic;
using SmartToggle.Models;
using Xunit;

namespace SmartToggle.Tests
{
    public class CompanyBusinessLogicTests
    {
        private readonly Mock<ICompanyRepository> _companyRepo = new();
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly CompanyBusinessLogic _sut;

        public CompanyBusinessLogicTests()
        {
            _sut = new CompanyBusinessLogic(_companyRepo.Object, _serviceRepo.Object);
        }

        [Fact]
        public async Task DeleteCompanyAsync_WhenServicesExist_ThrowsInvalidOperationException()
        {
            var companyId = "company-1";
            var services = new List<Service> { new Service { Id = "service-1" } };

            _serviceRepo.Setup(r => r.GetByCompanyIdAsync(companyId)).ReturnsAsync(services);

            await Assert.ThrowsAsync<System.InvalidOperationException>(
                () => _sut.DeleteCompanyAsync(companyId));
        }

        [Fact]
        public async Task DeleteCompanyAsync_WhenNoServices_DeletesSuccessfully()
        {
            var companyId = "company-1";

            _serviceRepo.Setup(r => r.GetByCompanyIdAsync(companyId)).ReturnsAsync(new List<Service>());
            _companyRepo.Setup(r => r.DeleteAsync(companyId)).ReturnsAsync(true);

            var result = await _sut.DeleteCompanyAsync(companyId);

            Assert.True(result);
        }

        [Fact]
        public async Task CreateCompanyAsync_WhenNameIsMissing_ThrowsArgumentException()
        {
            var company = new Company { Name = "" };

            await Assert.ThrowsAsync<System.Exception>(
                () => _sut.CreateCompanyAsync(company, "owner-1"));
        }

        [Fact]
        public async Task CreateCompanyAsync_WhenValid_ReturnsCreatedCompany()
        {
            var company = new Company { Name = "Test Co" };
            var created = new Company { Id = "company-1", Name = "Test Co", OwnerId = "owner-1" };

            _companyRepo.Setup(r => r.AddAsync(It.IsAny<Company>())).ReturnsAsync(created);

            var result = await _sut.CreateCompanyAsync(company, "owner-1");

            Assert.NotNull(result);
            Assert.Equal("company-1", result.Id);
        }

        [Fact]
        public async Task UpdateCompanyAsync_WhenCompanyNotFound_ReturnsNull()
        {
            _companyRepo.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((Company?)null);

            var result = await _sut.UpdateCompanyAsync("missing", new Company { Name = "Test" });

            Assert.Null(result);
        }

        [Fact]
        public async Task ProvisionCompanyAsync_WhenCompanyDoesNotExist_CreatesNewCompany()
        {
            var tenantId = "tenant-1";
            var created = new Company { Id = tenantId, Name = "Contoso", OwnerId = tenantId };

            _companyRepo.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync((Company?)null);
            _companyRepo.Setup(r => r.AddAsync(It.IsAny<Company>())).ReturnsAsync(created);

            var result = await _sut.ProvisionCompanyAsync(tenantId, "Contoso");

            Assert.Equal("Contoso", result.Name);
            _companyRepo.Verify(r => r.AddAsync(It.IsAny<Company>()), Times.Once);
        }

        [Fact]
        public async Task ProvisionCompanyAsync_WhenCompanyExistsWithMyOrganization_UpdatesName()
        {
            var tenantId = "tenant-1";
            var existing = new Company { Id = tenantId, Name = "My Organization", OwnerId = tenantId };

            _companyRepo.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync(existing);
            _companyRepo.Setup(r => r.UpdateAsync(It.IsAny<Company>())).ReturnsAsync(existing);

            var result = await _sut.ProvisionCompanyAsync(tenantId, "Contoso");

            Assert.Equal("Contoso", result.Name);
            _companyRepo.Verify(r => r.UpdateAsync(It.IsAny<Company>()), Times.Once);
        }

        [Fact]
        public async Task ProvisionCompanyAsync_WhenCompanyExistsWithRealName_DoesNotUpdate()
        {
            var tenantId = "tenant-1";
            var existing = new Company { Id = tenantId, Name = "Contoso", OwnerId = tenantId };

            _companyRepo.Setup(r => r.GetByIdAsync(tenantId)).ReturnsAsync(existing);

            var result = await _sut.ProvisionCompanyAsync(tenantId, "Contoso");

            Assert.Equal("Contoso", result.Name);
            _companyRepo.Verify(r => r.UpdateAsync(It.IsAny<Company>()), Times.Never);
        }
    }
}
