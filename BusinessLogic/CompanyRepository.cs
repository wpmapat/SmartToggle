using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly Container _container;
        private const string PartitionKeyPath = "/id";

        public CompanyRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        /// <summary>
        /// Get all companies from Cosmos DB
        /// </summary>
        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            try
            {
                var query = _container.GetItemQueryIterator<Company>(
                    new QueryDefinition("SELECT * FROM c"));

                var companies = new List<Company>();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    companies.AddRange(response);
                }

                return companies;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all companies from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Get company by ID from Cosmos DB
        /// </summary>
        public async Task<Company?> GetByIdAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Company>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving company with ID: {id} from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Add new company to Cosmos DB
        /// </summary>
        public async Task<Company> AddAsync(Company company)
        {
            try
            {
                if (company == null)
                    throw new ArgumentNullException(nameof(company));

                var response = await _container.CreateItemAsync(company, new PartitionKey(company.Id));
                return response.Resource;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating company in Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Update existing company in Cosmos DB
        /// </summary>
        public async Task<Company> UpdateAsync(Company company)
        {
            try
            {
                if (company == null)
                    throw new ArgumentNullException(nameof(company));

                var response = await _container.UpsertItemAsync(company, new PartitionKey(company.Id));
                return response.Resource;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating company with ID: {company.Id} in Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Delete company from Cosmos DB
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Company ID cannot be null or empty.", nameof(id));

                await _container.DeleteItemAsync<Company>(id, new PartitionKey(id));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting company with ID: {id} from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Search companies by name
        /// </summary>
        public async Task<IEnumerable<Company>> GetCompaniesByNameAsync(string name)
        {
            try
            {
                var query = _container.GetItemQueryIterator<Company>(
                    new QueryDefinition("SELECT * FROM c WHERE CONTAINS(c.name, @name)")
                        .WithParameter("@name", name));

                var companies = new List<Company>();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    companies.AddRange(response);
                }

                return companies;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching companies by name: {name} in Cosmos DB.", ex);
            }
        }
    }
}