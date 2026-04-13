using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public class FeatureFlagBusinessLogic : IFeatureFlagBusinessLogic
    {
        private readonly IFeatureFlagRepository _featureFlagRepository;
        private readonly IServiceRepository _serviceRepository;

        public FeatureFlagBusinessLogic(IFeatureFlagRepository featureFlagRepository, IServiceRepository serviceRepository)
        {
            _featureFlagRepository = featureFlagRepository;
            _serviceRepository = serviceRepository;
        }

        /// <summary>
        /// Get all feature flags
        /// </summary>
        public async Task<IEnumerable<FeatureFlag<bool>>> GetAllFeatureFlagsAsync()
        {
            try
            {
                return await _featureFlagRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all feature flags.", ex);
            }
        }

        /// <summary>
        /// Get feature flag by ID
        /// </summary>
        public async Task<FeatureFlag<bool>?> GetFeatureFlagByIdAsync(string id, string serviceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Feature flag ID cannot be null or empty.", nameof(id));

                var flag = await _featureFlagRepository.GetByIdAsync(id, serviceId);
                return flag;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving feature flag with ID: {id}", ex);
            }
        }

        /// <summary>
        /// Get feature flags by company ID
        /// </summary>
        public async Task<IEnumerable<FeatureFlag<bool>>> GetFeatureFlagsByCompanyIdAsync(string companyId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(companyId))
                    throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

                return await _featureFlagRepository.GetByCompanyIdAsync(companyId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving feature flags for company {companyId}", ex);
            }
        }

        /// <summary>
        /// Get feature flags by service ID
        /// </summary>
        public async Task<IEnumerable<FeatureFlag<bool>>> GetFeatureFlagsByServiceIdAsync(string serviceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serviceId))
                    throw new ArgumentException("Service ID cannot be null or empty.", nameof(serviceId));

                return await _featureFlagRepository.GetByServiceIdAsync(serviceId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving feature flags for service {serviceId}", ex);
            }
        }

        /// <summary>
        /// Get feature flag by flag ID
        /// </summary>
        /*public async Task<FeatureFlag<bool>?> GetFeatureFlagByFlagIdAsync(string flagId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(flagId))
                    throw new ArgumentException("Flag ID cannot be null or empty.", nameof(flagId));

                return await _featureFlagRepository.GetByFlagIdAsync(flagId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving feature flag with flagId: {flagId}", ex);
            }
        }
        */
        /// <summary>
        /// Create a new feature flag
        /// </summary>
        public async Task<FeatureFlag<bool>> CreateFeatureFlagAsync(FeatureFlag<bool> featureFlag)
        {
            try
            {
                if (featureFlag == null)
                    throw new ArgumentNullException(nameof(featureFlag), "Feature flag cannot be null.");

                if (string.IsNullOrWhiteSpace(featureFlag.FlagId))
                    throw new ArgumentException("Flag ID is required.", nameof(featureFlag.FlagId));

                if (string.IsNullOrWhiteSpace(featureFlag.CompanyId))
                    throw new ArgumentException("Company ID is required.", nameof(featureFlag.CompanyId));

                if (string.IsNullOrWhiteSpace(featureFlag.ServiceId))
                    throw new ArgumentException("Service ID is required.", nameof(featureFlag.ServiceId));

                var service = await _serviceRepository.GetByIdAsync(featureFlag.ServiceId);
                if (service == null)
                    throw new Exception($"Service with ID '{featureFlag.ServiceId}' does not exist.");

                featureFlag.Id = Guid.NewGuid().ToString();

                return await _featureFlagRepository.AddAsync(featureFlag);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating feature flag.", ex);
            }
        }

        /// <summary>
        /// Update an existing feature flag
        /// </summary>
        public async Task<FeatureFlag<bool>?> UpdateFeatureFlagAsync(string id, FeatureFlag<bool> featureFlag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Feature flag ID cannot be null or empty.", nameof(id));

                if (featureFlag == null)
                    throw new ArgumentNullException(nameof(featureFlag), "Feature flag cannot be null.");

                if (featureFlag.ServiceId == null)
                    throw new ArgumentNullException(nameof(featureFlag.ServiceId), "Service Id cannot be null.");

                var existingFlag = await _featureFlagRepository.GetByIdAsync(id, featureFlag.ServiceId);

                if (existingFlag == null)
                    return null;

                featureFlag.Id = id;
                return await _featureFlagRepository.UpdateAsync(featureFlag);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating feature flag with ID: {id}", ex);
            }
        }

        /// <summary>
        /// Delete a feature flag
        /// </summary>
        public async Task<bool> DeleteFeatureFlagAsync(string id, string serviceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Feature flag ID cannot be null or empty.", nameof(id));

                return await _featureFlagRepository.DeleteAsync(id, serviceId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting feature flag with ID: {id}", ex);
            }
        }
    }
}