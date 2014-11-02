using Microsoft.AspNet.Identity;

namespace BlueCore.Identity.AzureTableStorage
{
    /// <summary>
    ///     Azure table storage implementation of the Asp.Net Identity IRole entity.
    /// </summary>
    public class AzureTableRole : AzureTableEntityBase, IRole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableRole"/> class.
        /// </summary>
        public AzureTableRole()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AzureTableRole" /> class.
        /// </summary>
        /// <param name="name">The human-readable name of the role.</param>
        public AzureTableRole(string name) : base(name)
        {
        }
    }
}