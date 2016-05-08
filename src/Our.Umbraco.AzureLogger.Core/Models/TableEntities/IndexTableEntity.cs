namespace Our.Umbraco.AzureLogger.Core.Models.TableEntities
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// POCO to match the field names in the Azure table (NOTE: casing of properties matches the Azure field names)
    /// </summary>
    public class IndexTableEntity : TableEntity
    {
        /// <summary>
        /// Default Constructor, as required by the Azure storage api
        /// </summary>
        public IndexTableEntity()
        {
        }

        //internal IndexTableEntity(string partitionKey)
        //{
        //    this.PartitionKey = partitionKey;
        //}
    }
}
