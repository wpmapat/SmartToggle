using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public interface ICompanyBusinessLogic
    {
        Task<IEnumerable<Company>> GetAllCompaniesAsync();
        Task<Company?> GetCompanyByIdAsync(string id);
        Task<IEnumerable<Company?>> GetCompaniesByNameAsync(string name); 
        Task<Company> CreateCompanyAsync(Company company);
        Task<Company?> UpdateCompanyAsync(string id, Company company);
        Task<bool> DeleteCompanyAsync(string id);
    }
}