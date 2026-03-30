using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public class CompanyBusinessLogic : ICompanyBusinessLogic
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyBusinessLogic(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        /// <summary>
        /// Get all companies
        /// </summary>
        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
        {
            try
            {
                return await _companyRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all companies.", ex);
            }
        }

        /// <summary>
        /// Get company by ID
        /// </summary>
        public async Task<Company?> GetCompanyByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Company ID cannot be null or empty.", nameof(id));

                return await _companyRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving company with ID: {id}", ex);
            }
        }

        /// <summary>
        /// Create a new company
        /// </summary>
        public async Task<Company> CreateCompanyAsync(Company company)
        {
            try
            {
                if (company == null)
                    throw new ArgumentNullException(nameof(company), "Company cannot be null.");

                if (string.IsNullOrWhiteSpace(company.Name))
                    throw new ArgumentException("Company name is required.", nameof(company.Name));

                //company.Id = Guid.NewGuid().ToString();
                //company.CreatedAt = DateTime.UtcNow.ToString("o");

                return await _companyRepository.AddAsync(company);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating company.", ex);
            }
        }

        /// <summary>
        /// Update an existing company
        /// </summary>
        public async Task<Company?> UpdateCompanyAsync(string id, Company company)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Company ID cannot be null or empty.", nameof(id));

                if (company == null)
                    throw new ArgumentNullException(nameof(company), "Company cannot be null.");

                var existingCompany = await _companyRepository.GetByIdAsync(id);
                if (existingCompany == null)
                    return null;

                company.Id = id;
                return await _companyRepository.UpdateAsync(company);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating company with ID: {id}", ex);
            }
        }

        /// <summary>
        /// Delete a company
        /// </summary>
        public async Task<bool> DeleteCompanyAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Company ID cannot be null or empty.", nameof(id));

                return await _companyRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting company with ID: {id}", ex);
            }
        }

        /// <summary>
        /// Get companies by name
        /// </summary>
        public async Task<IEnumerable<Company>> GetCompaniesByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Company name cannot be null or empty.", nameof(name));

                return await _companyRepository.GetCompaniesByNameAsync(name);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching companies by name: {name}", ex);
            }
        }
    }
}