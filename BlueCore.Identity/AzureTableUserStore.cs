using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace BlueCore.Identity.AzureTableStorage
{
    public class AzureTableUserStore<T> : AzureTableStoreBase<T>, IUserStore<T>, IUserClaimStore<T>,
        IQueryableUserStore<T>, IUserRoleStore<T>,
        IUserTwoFactorStore<T, string>, IUserEmailStore<T>, IUserLockoutStore<T, string>, IUserLoginStore<T>,
        IUserPasswordStore<T>, IUserPhoneNumberStore<T>, IUserSecurityStampStore<T>
        where T : AzureTableUser, new()
    {
        public const string DefaultTableName = "Users";
        public const string DefaultLoginTableName = "Logins";

        private readonly CloudTable _loginsTable;

        public AzureTableUserStore(string connectionStringAppSetting = DefaultConnectionStringAppSettingName,
            string tablePrefix = DefaultTablePrefix, string tableName = DefaultTableName,
            string loginTableName = DefaultLoginTableName)
            : this(
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings[connectionStringAppSetting]), tablePrefix,
                tableName, loginTableName)
        {
        }


        public AzureTableUserStore(CloudStorageAccount storageAccount, string tablePrefix = DefaultTablePrefix,
            string tableName = DefaultTableName, string loginTableName = DefaultLoginTableName)
            : base(tableName, storageAccount, tablePrefix)
        {
            _loginsTable = TableClient.GetTableReference(tablePrefix + loginTableName);
            _loginsTable.CreateIfNotExists();
        }

        public IQueryable<T> Users
        {
            get { return All; }
        }

        public Task AddClaimAsync(T user, Claim claim)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (claim == null) throw new ArgumentNullException("claim");
            user.AddClaim(claim);
            return Task.FromResult(0);
        }

        public Task AddLoginAsync(T user, UserLoginInfo login)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (login == null) throw new ArgumentNullException("login");
            user.AddLogin(login.LoginProvider, login.ProviderKey);
            return Task.FromResult(0);
        }

        public Task AddToRoleAsync(T user, string roleName)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("roleName cannot be null or empty.", "roleName");
            user.Roles.Add(roleName);
            return Task.FromResult(0);
        }

        public override async Task CreateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            await base.CreateAsync(entity);

            await CreateLogins(entity.NewLogins, entity.Id);
        }

        public override async Task DeleteAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            var batch = new TableBatchOperation();
            foreach (UserLoginInfo loginInfo in entity.OldLogins)
            {
                var login = new AzureTableUserLogin(loginInfo.LoginProvider, loginInfo.ProviderKey, entity.Id);
                TableOperation operation = TableOperation.Insert(login);
                batch.Add(operation);
            }
            await _loginsTable.ExecuteBatchAsync(batch);

            await base.DeleteAsync(entity);
        }

        public async Task<T> FindAsync(UserLoginInfo loginInfo)
        {
            if (loginInfo == null) throw new ArgumentNullException("loginInfo");

            if (string.IsNullOrWhiteSpace(loginInfo.LoginProvider))
                throw new ArgumentException("loginInfo.LoginProvider cannot be null or empty.", "loginInfo");
            if (string.IsNullOrWhiteSpace(loginInfo.ProviderKey))
                throw new ArgumentException("loginInfo.ProviderKey cannot be null or empty.", "loginInfo");

            var operation = TableOperation.Retrieve<AzureTableUserLogin>(loginInfo.LoginProvider, loginInfo.ProviderKey);
            TableResult result = await _loginsTable.ExecuteAsync(operation);

            if (result.Result == null)
            {
                return null;
            }

            var login = (AzureTableUserLogin) result.Result;

            return await FindByIdAsync(login.UserId);
        }

        public async Task<T> FindByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("email cannot be null or empty.", "email");
            return await FindByNameAsync(email);
        }

        public Task<int> GetAccessFailedCountAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<IList<Claim>> GetClaimsAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            // ReSharper disable once RedundantEnumerableCastCall
            return Task.FromResult(user.Claims);
        }

        public Task<string> GetEmailAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<bool> GetLockoutEnabledAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return
                Task.FromResult(user.LockoutEndDateUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset());
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.NewLogins);
        }

        public Task<string> GetPasswordHashAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetPhoneNumberAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task<IList<string>> GetRolesAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult((IList<string>) user.Roles);
        }

        public Task<string> GetSecurityStampAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.SecurityStamp ?? string.Empty);
        }

        public Task<bool> GetTwoFactorEnabledAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task<bool> HasPasswordAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task<int> IncrementAccessFailedCountAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        public async Task<bool> IsInRoleAsync(T user, string roleName)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("roleName cannot be null or empty.", "roleName");
            return
                await
                    Task<bool>.Factory.StartNew(
                        () => user.Roles.Any(x => x.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)));
        }

        public async Task RemoveClaimAsync(T user, Claim claim)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (claim == null) throw new ArgumentNullException("claim");
            user.RemoveClaim(claim.Type, claim.Value);
            await UpdateAsync(user);
        }

        public async Task RemoveFromRoleAsync(T user, string roleName)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("roleName cannot be null or empty.", "roleName");
            user.Roles.Remove(roleName);
            await UpdateAsync(user);
        }

        public async Task RemoveLoginAsync(T user, UserLoginInfo login)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (login == null) throw new ArgumentNullException("login");
            user.RemoveLogin(login.LoginProvider, login.ProviderKey);
            await UpdateAsync(user);
        }

        public Task ResetAccessFailedCountAsync(T user)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(T user, string email)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("email cannot be null or empty.", "email");
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(T user, bool confirmed)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(T user, bool enabled)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task SetLockoutEndDateAsync(T user, DateTimeOffset lockoutEnd)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? (DateTime?) null : lockoutEnd.UtcDateTime;
            return Task.FromResult(0);
        }

        public Task SetPasswordHashAsync(T user, string passwordHash)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberAsync(T user, string phoneNumber)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberConfirmedAsync(T user, bool confirmed)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task SetSecurityStampAsync(T user, string stamp)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(T user, bool enabled)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public override async Task UpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            await base.UpdateAsync(entity);

            IEnumerable<UserLoginInfo> logins = entity.NewLogins.Except(entity.OldLogins);
            await CreateLogins(logins, entity.Id);

            entity.Commit();
        }

        private async Task CreateLogins(IEnumerable<UserLoginInfo> logins, string userId)
        {
            if (logins == null) throw new ArgumentNullException("logins");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId cannot be null or empty.", "userId");
            var batch = new TableBatchOperation();
            foreach (UserLoginInfo loginInfo in logins)
            {
                var login = new AzureTableUserLogin(loginInfo.LoginProvider, loginInfo.ProviderKey, userId);
                TableOperation operation = TableOperation.Insert(login);
                batch.Add(operation);
            }
            if (batch.Count == 0)
            {
                return;
            }
            await _loginsTable.ExecuteBatchAsync(batch);
        }
    }
}