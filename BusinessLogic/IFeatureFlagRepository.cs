using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public interface IFeatureFlagRepository
    {
        Task<IEnumerable<FeatureFlag<bool>>> GetAllAsync();
        Task<FeatureFlag<bool>?> GetByIdAsync(string id, string serviceId);
        Task<IEnumerable<FeatureFlag<bool>>> GetByCompanyIdAsync(string companyId);
        Task<IEnumerable<FeatureFlag<bool>>> GetByServiceIdAsync(string serviceId);
        Task<FeatureFlag<bool>?> GetByFlagIdAsync(string flagId);
        Task<FeatureFlag<bool>> AddAsync(FeatureFlag<bool> featureFlag);
        Task<FeatureFlag<bool>> UpdateAsync(FeatureFlag<bool> featureFlag);
        Task<bool> DeleteAsync(string id, string serviceId);
    }
}