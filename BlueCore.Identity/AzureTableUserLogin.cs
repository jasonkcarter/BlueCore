using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage.Table;

namespace BlueCore.Identity.AzureTableStorage
{
    public class AzureTableUserLogin : TableEntity
    {
        public AzureTableUserLogin()
        {
        }

        public AzureTableUserLogin(string loginProvider, string providerKey, string userId)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            UserId = userId;
        }

        [IgnoreProperty]
        public bool IsNew { get; set; }

        public string LoginProvider
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string ProviderKey
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string UserId { get; set; }

        public static implicit operator UserLoginInfo(AzureTableUserLogin login)
        {
            return new UserLoginInfo(login.LoginProvider, login.ProviderKey);
        }
    }
}