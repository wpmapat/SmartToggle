using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly Container _container;

        public ServiceRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        /// <summary>
        /// Get all services from Cosmos DB
        /// </summary>
        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            try
            {
                var query = _container.GetItemQueryIterator<Service>(
                    new QueryDefinition("SELECT * FROM c"));

                var services = new List<Service>();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    services.AddRange(response);
                }

                return services;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all services from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Get service by ID from Cosmos DB
        /// </summary>
        public async Task<Service?> GetByIdAsync(string id)
        {
            try
            {
                var query = _container.GetItemQueryIterator<Service>(
                    new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                        .WithParameter("@id", id));

                var response = await query.ReadNextAsync();
                return response.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service with ID: {id} from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Get services by company ID
        /// </summary>
        public async Task<IEnumerable<Service>> GetByCompanyIdAsync(string companyId)
        {
            try
            {
                var query = _container.GetItemQueryIterator<Service>(
                    new QueryDefinition("SELECT * FROM c WHERE c.companyId = @companyId")
                        .WithParameter("@companyId", companyId));

                var services = new List<Service>();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    services.AddRange(response);
                }

                return services;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving services for company {companyId} from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Add new service to Cosmos DB
        /// </summary>
        public async Task<Service> AddAsync(Service service)
        {
            try
            {
                if (service == null)
                    throw new ArgumentNullException(nameof(service));

                var response = await _container.CreateItemAsync(service, new PartitionKey(service.CompanyId));
                return response.Resource;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating service in Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Update existing service in Cosmos DB
        /// </summary>
        public async Task<Service> UpdateAsync(Service service)
        {
            try
            {
                if (service == null)
                    throw new ArgumentNullException(nameof(service));

                var response = await _container.UpsertItemAsync(service, new PartitionKey(service.CompanyId));
                return response.Resource;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating service with ID: {service.Id} in Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Delete service from Cosmos DB
        /// </summary>
        public async Task<bool> DeleteAsync(string id, string companyId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Service ID cannot be null or empty.", nameof(id));

                if (string.IsNullOrWhiteSpace(companyId))
                    throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

                await _container.DeleteItemAsync<Service>(id, new PartitionKey(companyId));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting service with ID: {id} from Cosmos DB.", ex);
            }
        }
    }
}