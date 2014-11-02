using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage;

namespace BlueCore.Identity.AzureTableStorage
{
    public class AzureTableRoleStore<T> : AzureTableStoreBase<T>, IRoleStore<T>, IQueryableRoleStore<T>
        where T : AzureTableRole, new()
    {
        public const string DefaultTableName = "Roles";

        public AzureTableRoleStore(string connectionStringAppSetting = DefaultConnectionStringAppSettingName,
            string tablePrefix = DefaultTablePrefix, string tableName = DefaultTableName)
            : base(tableName, connectionStringAppSetting, tablePrefix)
        {
        }

        public AzureTableRoleStore(CloudStorageAccount storageAccount, string tablePrefix = DefaultTablePrefix,
            string tableName = DefaultTableName) : base(tableName, storageAccount, tablePrefix)
        {
        }

        public IQueryable<T> Roles
        {
            get { return All; }
        }
    }
}