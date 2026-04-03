using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public interface IFeatureFlagBusinessLogic
    {
        Task<IEnumerable<FeatureFlag<bool>>> GetAllFeatureFlagsAsync();
        Task<FeatureFlag<bool>?> GetFeatureFlagByIdAsync(string id,string serviceId);
        Task<IEnumerable<FeatureFlag<bool>>> GetFeatureFlagsByCompanyIdAsync(string companyId);
        Task<IEnumerable<FeatureFlag<bool>>> GetFeatureFlagsByServiceIdAsync(string serviceId);
        //Task<FeatureFlag<bool>?> GetFeatureFlagByFlagIdAsync(string flagId, string serviceId);
        Task<FeatureFlag<bool>> CreateFeatureFlagAsync(FeatureFlag<bool> featureFlag);
        Task<FeatureFlag<bool>?> UpdateFeatureFlagAsync(string id, FeatureFlag<bool> featureFlag);
        Task<bool> DeleteFeatureFlagAsync(string id, string serviceId);
    }
}