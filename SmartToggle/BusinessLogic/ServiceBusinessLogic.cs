using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartToggle.Models;
using SmartToggle.BusinessLogic;

namespace SmartToggle.BusinessLogic
{
    public class ServiceBusinessLogic : IServiceBusinessLogic
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFeatureFlagRepository _featureFlagRepository;

        public ServiceBusinessLogic(IServiceRepository serviceRepository, ICompanyRepository companyRepository, IFeatureFlagRepository featureFlagRepository)
        {
            _serviceRepository = serviceRepository;
            _companyRepository = companyRepository;
            _featureFlagRepository = featureFlagRepository;
        }

        /// <summary>
        /// Get all services
        /// </summary>
        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            try
            {
                return await _serviceRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all services.", ex);
            }
        }

        /// <summary>
        /// Get service by ID
        /// </summary>
        public async Task<Service?> GetServiceByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Service ID cannot be null or empty.", nameof(id));

                return await _serviceRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service with ID: {id}", ex);
            }
        }

        /// <summary>
        /// Get services by company ID
        /// </summary>
        public async Task<IEnumerable<Service>> GetServicesByCompanyIdAsync(string companyId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(companyId))
                    throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

                return await _serviceRepository.GetByCompanyIdAsync(companyId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving services for company {companyId}", ex);
            }
        }

        /// <summary>
        /// Create a new service
        /// </summary>
        public async Task<Service> CreateServiceAsync(Service service)
        {
            try
            {
                if (service == null)
                    throw new ArgumentNullException(nameof(service), "Service cannot be null.");

                if (string.IsNullOrWhiteSpace(service.ServiceName))
                    throw new ArgumentException("Service name is required.", nameof(service.ServiceName));

                if (string.IsNullOrWhiteSpace(service.CompanyId))
                    throw new ArgumentException("Company ID is required.", nameof(service.CompanyId));

                var company = await _companyRepository.GetByIdAsync(service.CompanyId);
                if (company == null)
                    throw new Exception($"Company with ID '{service.CompanyId}' does not exist.");

                service.Id = Guid.NewGuid().ToString();

                return await _serviceRepository.AddAsync(service);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating service.", ex);
            }
        }

        /// <summary>
        /// Update an existing service
        /// </summary>
        public async Task<Service?> UpdateServiceAsync(string id, Service service)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Service ID cannot be null or empty.", nameof(id));

                if (service == null)
                    throw new ArgumentNullException(nameof(service), "Service cannot be null.");

                var existingService = await _serviceRepository.GetByIdAsync(id);
                if (existingService == null)
                    return null;

                service.Id = id;
                service.CompanyId = existingService.CompanyId;
                return await _serviceRepository.UpdateAsync(service);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating service with ID: {id}", ex);
            }
        }

        /// <summary>
        /// Delete a service
        /// </summary>
        public async Task<bool> DeleteServiceAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Service ID cannot be null or empty.", nameof(id));

                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null)
                    return false;

                var featureFlags = await _featureFlagRepository.GetByServiceIdAsync(id);
                if (featureFlags != null && featureFlags.Any())
                    throw new InvalidOperationException($"Cannot delete service '{id}' because it has associated feature flags. Please delete all feature flags for this service first.");

                return await _serviceRepository.DeleteAsync(id, service.CompanyId);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting service with ID: {id}", ex);
            }
        }
    }
}