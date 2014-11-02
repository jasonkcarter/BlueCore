using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlueCore.Identity.AzureTableStorage
{
    public static class AzureTableHelper
    {
        /// <summary>
        ///     Computes a statistically-random, but deterministic partition key for a given table entity id.
        /// </summary>
        /// <param name="id">The id of the entity to create the partition key for.</param>
        public static string ComputePartitionKey(string id)
        {
            // Use the first character of the role id's MD5 hash as the partition key 
            // in order to ensure statistically even partition distribution
            byte[] idBytes = Encoding.Unicode.GetBytes(id);
            return
                MD5.Create()
                    .ComputeHash(idBytes)
                    .First()
                    .ToString(CultureInfo.InvariantCulture);
        }
    }
}