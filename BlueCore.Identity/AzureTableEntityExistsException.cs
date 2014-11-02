using System;

namespace BlueCore.Identity.AzureTableStorage
{
    /// <summary>
    ///     Thrown when trying to create a role with a name that conflicts with an existing role.
    /// </summary>
    public class AzureTableEntityExistsException : ApplicationException
    {
        public AzureTableEntityExistsException(AzureTableEntityBase entity, Exception innerException = null)
            : base(string.Format("An entity with the name '{0}' already exists.", entity.Name), innerException)
        {
        }
    }
}