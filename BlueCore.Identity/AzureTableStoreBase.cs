using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace BlueCore.Identity.AzureTableStorage
{
    public abstract class AzureTableStoreBase<T> where T : AzureTableEntityBase, new()
    {
        public const string DefaultConnectionStringAppSettingName = "AzureTableStoreConnectionString";
        public const string DefaultTablePrefix = "AspNet";

        protected readonly CloudTable Table;
        protected readonly CloudTableClient TableClient;

        protected AzureTableStoreBase(string tableName, string connectionStringAppSetting, string tablePrefix)
            : this(
                tableName, CloudStorageAccount.Parse(ConfigurationManager.AppSettings[connectionStringAppSetting]),
                tablePrefix)
        {
        }

        protected AzureTableStoreBase(string tableName, CloudStorageAccount storageAccount, string tablePrefix)
        {
            if (storageAccount == null) throw new ArgumentNullException("storageAccount");
            if (tablePrefix == null) throw new ArgumentNullException("tablePrefix");
            if (tableName == null) throw new ArgumentNullException("tableName");
            TableClient = storageAccount.CreateCloudTableClient();
            Table = TableClient.GetTableReference(tablePrefix + tableName);
            Table.CreateIfNotExists();
        }

        public IQueryable<T> All
        {
            get
            {
                var query = new TableQuery<T>();
                return Table.ExecuteQuery(query).AsQueryable();
            }
        }

        public virtual async Task CreateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var operation = TableOperation.Insert(entity);
            try
            {
                await Table.ExecuteAsync(operation);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int) HttpStatusCode.Conflict)
                {
                    throw new AzureTableEntityExistsException(entity);
                }
                throw;
            }
        }

        public virtual async Task DeleteAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            entity.ETag = "*";
            var operation = TableOperation.Delete(entity);
            await Table.ExecuteAsync(operation);
        }

        public void Dispose()
        {
        }

        public async Task<T> FindByIdAsync(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentOutOfRangeException("entityId", "entityId cannot be empty.");
            string partitionKey = AzureTableHelper.ComputePartitionKey(entityId);
            string rowKey = entityId;

            var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            TableResult result = await Table.ExecuteAsync(operation);
            return (T) result.Result;
        }

        public async Task<T> FindByNameAsync(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentOutOfRangeException("entityName", "entityName cannot be empty.");
            string roleId = Convert.ToBase64String(Encoding.Unicode.GetBytes(entityName));

            return await FindByIdAsync(roleId);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            // Name changed
            // Need to create a new entity with the new name and delete the old one
            if (entity.Name != entity.OriginalName)
            {
                await CreateAsync(entity);

                var oldEntity = new T {Name = entity.OriginalName};

                await DeleteAsync(oldEntity);
                return;
            }

            // Name the same. Just do a replace.
            var operation = TableOperation.Replace(entity);
            await Table.ExecuteAsync(operation);
        }
    }
}