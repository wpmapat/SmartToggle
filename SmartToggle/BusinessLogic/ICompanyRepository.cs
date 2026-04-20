using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<Company>> GetAllAsync();
        Task<IEnumerable<Company>> GetByOwnerIdAsync(string ownerId);
        Task<Company?> GetByIdAsync(string id);
        Task<Company> AddAsync(Company company);
        Task<Company> UpdateAsync(Company company);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<Company>> GetCompaniesByNameAsync(string name);
    }
}