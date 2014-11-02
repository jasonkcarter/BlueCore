using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace BlueCore.Identity.AzureTableStorage
{
    public class AzureTableEntityBase : TableEntity
    {
        private string _name;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AzureTableEntityBase" /> class.
        /// </summary>
        public AzureTableEntityBase()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AzureTableEntityBase" /> class.
        /// </summary>
        /// <param name="name">The human-readable name of the entity.</param>
        public AzureTableEntityBase(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Gets the unique identifier for this entity.  Same as the Name, only base64-encoded.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this entity is enabled.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        ///     Gets or sets the human-readable name of the entity.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentOutOfRangeException("value", "Name cannot be empty.");

                // Save the original name for change tracking purposes
                if (_name == null)
                {
                    OriginalName = value;
                }
                _name = value;

                /* The id/rowkey will be the same as the name, only base64 encoded.
                 * This avoids having to maintain a separate table for name lookups.
                 * If the username changes, we'll basically just create a new user 
                 * and delete the old one. */
                byte[] nameBytes = Encoding.Unicode.GetBytes(value);
                Id = Convert.ToBase64String(nameBytes);

                PartitionKey = AzureTableHelper.ComputePartitionKey(Id);
                RowKey = Id;
            }
        }

        /// <summary>
        ///     Gets or sets the human-readable name of the entity as it was when it was first loaded.
        /// </summary>
        public string OriginalName { get; private set; }

        protected T[] DeserializeListProperty<T>(IDictionary<string, EntityProperty> properties, string propertyName)
        {
            // Deserialize property collection
            EntityProperty property;

            if (properties.TryGetValue(propertyName, out property))
            {
                string json = property.StringValue;
                return JsonConvert.DeserializeObject<T[]>(json, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            }

            return null;
        }

        protected void SerializeListProperty<T>(IDictionary<string, EntityProperty> results, string propertyName,
            List<T> propertyValue)
        {
            // Serialize property collection
            string json = JsonConvert.SerializeObject(propertyValue, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var property = new EntityProperty(json);
            results.Add(propertyName, property);
        }
    }
}