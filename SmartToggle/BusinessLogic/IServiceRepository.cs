using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(string id);
        Task<IEnumerable<Service>> GetByCompanyIdAsync(string companyId);
        Task<Service> AddAsync(Service service);
        Task<Service> UpdateAsync(Service service);
        Task<bool> DeleteAsync(string id, string companyId);
    }
}