using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace BlueCore.Identity.AzureTableStorage
{
    /// <summary>
    ///     Azure table storage implementation of the Asp.Net Identity IUser entity.
    /// </summary>
    public class AzureTableUser : AzureTableEntityBase, IUser
    {
        private readonly List<AzureTableUserClaim> _claims = new List<AzureTableUserClaim>();
        private readonly List<UserLoginInfo> _logins = new List<UserLoginInfo>();
        private List<UserLoginInfo> _originalLogins = new List<UserLoginInfo>();


        /// <summary>
        ///     Initializes a new instance of the <see cref="AzureTableUser" /> class.
        /// </summary>
        public AzureTableUser()
        {
            Roles = new List<string>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AzureTableUser" /> class.
        /// </summary>
        /// <param name="name">The human-readable name of the entity.</param>
        public AzureTableUser(string name) : base(name)
        {
        }

        /// <summary>
        ///     Used to record failures for the purposes of lockout
        /// </summary>
        public int AccessFailedCount { get; set; }

        public IList<Claim> Claims
        {
            get { return _claims.Cast<Claim>().ToArray(); }
        }

        /// <summary>
        ///     The user's email address.
        /// </summary>
        public string Email
        {
            get { return Name; }
            set { Name = value; }
        }

        /// <summary>
        ///     True if the email is confirmed, default is false
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        ///     Is lockout enabled for this user
        /// </summary>
        public bool LockoutEnabled { get; set; }

        /// <summary>
        ///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public DateTime? LockoutEndDateUtc { get; set; }

        public IList<UserLoginInfo> NewLogins
        {
            get { return _logins.ToArray(); }
        }

        public IList<UserLoginInfo> OldLogins
        {
            get { return _originalLogins.ToArray(); }
        }

        /// <summary>
        ///     The salted/hashed form of the user password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        ///     PhoneNumber for the user
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        ///     True if the phone number is confirmed, default is false
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; }

        public List<string> Roles { get; private set; }

        /// <summary>
        ///     A random value that should change whenever a users credentials have changed (password changed, login removed)
        /// </summary>
        public string SecurityStamp { get; set; }

        /// <summary>
        ///     Is two factor enabled for the user
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the name of the during the process of changing the username.
        /// </summary>
        public string UserName
        {
            get { return Name; }
            set { Name = value; }
        }

        public AzureTableUserClaim AddClaim(Claim claim)
        {
            var newClaim = new AzureTableUserClaim(claim.Type, claim.Value, claim.ValueType, claim.Issuer,
                claim.OriginalIssuer, claim.Subject);
            _claims.Add(newClaim);
            return newClaim;
        }

        public UserLoginInfo AddLogin(string loginProvider, string providerKey)
        {
            var newLogin = new UserLoginInfo(loginProvider, providerKey);
            _logins.Add(newLogin);
            return newLogin;
        }

        public void Commit()
        {
            _originalLogins = new List<UserLoginInfo>(_logins);
        }

        /// <summary>
        ///     Deserializes the entity using the specified
        ///     <see cref="T:System.Collections.Generic.IDictionary`2" /> that maps property names to typed
        ///     <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty" /> values.
        /// </summary>
        /// <param name="properties">
        ///     An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that maps property names
        ///     to typed <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty" /> values.
        /// </param>
        /// <param name="operationContext">
        ///     An <see cref="T:Microsoft.WindowsAzure.Storage.OperationContext" /> object that
        ///     represents the context for the current operation.
        /// </param>
        public override void ReadEntity(IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            // Call the default deserialization logic
            base.ReadEntity(properties, operationContext);

            // Deserialize claims collection
            AzureTableUserClaim[] claims = DeserializeListProperty<AzureTableUserClaim>(properties, "Claims");
            if (claims != null)
            {
                _claims.AddRange(claims);
            }

            // Deserialize roles collection
            string[] roles = DeserializeListProperty<string>(properties, "Roles");
            if (roles != null)
            {
                Roles.AddRange(roles);
            }

            // Deserialize logins collection
            UserLoginInfo[] logins = DeserializeListProperty<UserLoginInfo>(properties, "_logins");
            if (logins != null)
            {
                _logins.AddRange(logins);
                _originalLogins.AddRange(logins);
            }
        }

        public int RemoveClaim(string claimType, string claimValue)
        {
            return _claims.RemoveAll(x => x.Type == claimType && x.Value == claimValue);
        }

        public int RemoveLogin(string loginProvider, string providerKey)
        {
            return _logins.RemoveAll(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
        }


        /// <summary>
        ///     Serializes the
        ///     <see cref="T:System.Collections.Generic.IDictionary`2" /> of property names mapped to
        ///     <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty" /> data values from this
        ///     <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> instance.
        /// </summary>
        /// <param name="operationContext">
        ///     An <see cref="T:Microsoft.WindowsAzure.Storage.OperationContext" /> object that
        ///     represents the context for the current operation.
        /// </param>
        /// <returns>
        ///     An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that maps string property names to
        ///     <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty" /> typed values created by serializing this table
        ///     entity instance.
        /// </returns>
        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            // Call the default serialization logic
            IDictionary<string, EntityProperty> results = base.WriteEntity(operationContext);

            // Serialize claims collection
            SerializeListProperty(results, "Claims", _claims);

            // Serialize roles collection
            SerializeListProperty(results, "Roles", Roles);

            // Serialize roles collection
            SerializeListProperty(results, "_logins", _logins);

            return results;
        }
    }
}