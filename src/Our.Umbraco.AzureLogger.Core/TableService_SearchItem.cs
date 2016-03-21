namespace Our.Umbraco.AzureLogger.Core
{
    using Microsoft.WindowsAzure.Storage.Table;
    using Models;
    using Models.TableEntities;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed partial class TableService
    {
        /// <summary>
        /// Create a new search item
        /// </summary>
        /// <param name="name"></param>
        internal void CreateSearchItemTableEntity(string name)
        {
            this.Connect();
            if (this.Connected.HasValue && this.Connected.Value)
            {
                this.CloudTable.Execute(
                    TableOperation.Insert(
                        new SearchItemTableEntity()
                        {
                            PartitionKey = TableService.SearchItemPartitionKey,
                            RowKey = Guid.NewGuid().ToString(),
                            Name = name,
                            HostName = null,
                            LoggerNamesInclude = false,
                            LoggerNames = string.Empty
                        }));
            }
        }

        internal IEnumerable<SearchItemTableEntity> ReadSearchItemTableEntities()
        {
            this.Connect();
            return this.Connected.HasValue && this.Connected.Value // if connected
                    ? this.CloudTable.ExecuteQuery(new TableQuery<SearchItemTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, TableService.SearchItemPartitionKey)))
                    : Enumerable.Empty<SearchItemTableEntity>();
        }

        internal SearchItemTableEntity ReadSearchItemTableEntity(string rowKey)
        {
            this.Connect();
            return this.Connected.HasValue && this.Connected.Value // if connected
                    ? this.CloudTable
                        .Execute(TableOperation.Retrieve<SearchItemTableEntity>(TableService.SearchItemPartitionKey, rowKey))
                        .Result as SearchItemTableEntity
                    : null;
        }

        internal void UpdateSearchItemTableEntity(string rowKey, Level minLevel, string hostName, bool loggerNamesInclude, string[] loggerNames)
        {
            this.Connect();
            if (this.Connected.HasValue && this.Connected.Value)
            {
                SearchItemTableEntity searchItemTableEntity = this.ReadSearchItemTableEntity(rowKey);
                searchItemTableEntity.MinLevel = minLevel.ToString();
                searchItemTableEntity.HostName = hostName;
                searchItemTableEntity.LoggerNamesInclude = loggerNamesInclude;
                searchItemTableEntity.LoggerNames = string.Join("|", loggerNames); // HACK: quick and dirty serialization - will error if logger names contain pipe | characters

                this.CloudTable.Execute(TableOperation.Replace(searchItemTableEntity));
            }
        }

        /// <summary>
        /// Deletes a search item form the Azure table (only a rowKey is required, so no need to supply a fully populated SearchItemTableEntity)
        /// </summary>
        /// <param name="rowKey">the searchItemId is the Azure table rowKey</param>
        internal void DeleteSearchItemTableEntity(string rowKey)
        {
            this.Connect();
            if (this.Connected.HasValue && this.Connected.Value)
            {
                this.CloudTable.Execute(TableOperation.Delete(new DynamicTableEntity(TableService.SearchItemPartitionKey, rowKey) { ETag = "*" }));
            }
        }

    }
}
