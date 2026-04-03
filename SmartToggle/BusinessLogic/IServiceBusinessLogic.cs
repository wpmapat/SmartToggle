using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public interface IServiceBusinessLogic
    {
        Task<IEnumerable<Service>> GetAllServicesAsync();
        Task<Service?> GetServiceByIdAsync(string id);
        Task<IEnumerable<Service>> GetServicesByCompanyIdAsync(string companyId);
        Task<Service> CreateServiceAsync(Service service);
        Task<Service?> UpdateServiceAsync(string id, Service service);
        Task<bool> DeleteServiceAsync(string id);
    }
}

