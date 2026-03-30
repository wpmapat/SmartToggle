using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using SmartToggle.Models;

namespace SmartToggle.BusinessLogic
{
    public class FeatureFlagRepository : IFeatureFlagRepository
    {
        private readonly Container _container;

        public FeatureFlagRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        /// <summary>
        /// Get all feature flags from Cosmos DB
        /// </summary>
        public async Task<IEnumerable<FeatureFlag<bool>>> GetAllAsync()
        {
            try
            {
                var query = _container.GetItemQueryIterator<FeatureFlag<bool>>(
                    new QueryDefinition("SELECT * FROM c"));

                var flags = new List<FeatureFlag<bool>>();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    flags.AddRange(response);
                }

                return flags;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all feature flags from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Get feature flag by ID from Cosmos DB
        /// </summary>
        public async Task<FeatureFlag<bool>?> GetByIdAsync(string id, string serviceId)
        {
            try
            {
                var response = await _container.ReadItemAsync<FeatureFlag<bool>>(id, new PartitionKey(serviceId));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving feature flag with ID: {id} from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Get feature flags by company ID
        /// </summary>
        public async Task<IEnumerable<FeatureFlag<bool>>> GetByCompanyIdAsync(string companyId)
        {
            try
            {
                var query = _container.GetItemQueryIterator<FeatureFlag<bool>>(
                    new QueryDefinition("SELECT * FROM c WHERE c.companyId = @companyId")
                        .WithParameter("@companyId", companyId));

                var flags = new List<FeatureFlag<bool>>();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    flags.AddRange(response);
                }

                return flags;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving feature flags for company {companyId} from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Get feature flags by service ID
        /// </summary>
        public async Task<IEnumerable<FeatureFlag<bool>>> GetByServiceIdAsync(string serviceId)
        {
            try
            {
                var query = _container.GetItemQueryIterator<FeatureFlag<bool>>(
                    new QueryDefinition("SELECT * FROM c WHERE c.serviceId = @serviceId")
                        .WithParameter("@serviceId", serviceId));

                var flags = new List<FeatureFlag<bool>>();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    flags.AddRange(response);
                }

                return flags;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving feature flags for service {serviceId} from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Get feature flag by flag ID
        /// </summary>
        public async Task<FeatureFlag<bool>?> GetByFlagIdAsync(string flagId)
        {
            try
            {
                var query = _container.GetItemQueryIterator<FeatureFlag<bool>>(
                    new QueryDefinition("SELECT * FROM c WHERE c.flagId = @flagId")
                        .WithParameter("@flagId", flagId));

                var response = await query.ReadNextAsync();
                return response.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving feature flag with flagId: {flagId} from Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Add new feature flag to Cosmos DB
        /// </summary>
        public async Task<FeatureFlag<bool>> AddAsync(FeatureFlag<bool> featureFlag)
        {
            try
            {
                if (featureFlag == null)
                    throw new ArgumentNullException(nameof(featureFlag));

                var response = await _container.CreateItemAsync(featureFlag, new PartitionKey(featureFlag.ServiceId));
                return response.Resource;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating feature flag in Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Update existing feature flag in Cosmos DB
        /// </summary>
        public async Task<FeatureFlag<bool>> UpdateAsync(FeatureFlag<bool> featureFlag)
        {
            try
            {
                if (featureFlag == null)
                    throw new ArgumentNullException(nameof(featureFlag));

                var response = await _container.UpsertItemAsync(featureFlag, new PartitionKey(featureFlag.ServiceId));
                return response.Resource;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating feature flag with ID: {featureFlag.Id} in Cosmos DB.", ex);
            }
        }

        /// <summary>
        /// Delete feature flag from Cosmos DB
        /// </summary>
        public async Task<bool> DeleteAsync(string id, string serviceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Feature flag ID cannot be null or empty.", nameof(id));

                if (string.IsNullOrWhiteSpace(serviceId))
                    throw new ArgumentException("Service ID cannot be null or empty.", nameof(serviceId));

                await _container.DeleteItemAsync<FeatureFlag<bool>>(id, new PartitionKey(serviceId));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting feature flag with ID: {id} from Cosmos DB.", ex);
            }
        }
    }
}