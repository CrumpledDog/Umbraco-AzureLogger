namespace UmbracoAzureLogger.Core
{
    using log4net;
    using log4net.Appender.Umbraco;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UmbracoAzureLogger.Core.Extensions;
    using UmbracoAzureLogger.Core.Models;
    using UmbracoAzureLogger.Core.Models.TableEntities;

    internal sealed class TableService
    {
        private static readonly TableService tableService = new TableService();

        static TableService()
        {
        }

        /// <summary>
        /// singleton constructor - construct a reference to the CloudTable property once
        /// </summary>
        private TableService()
        {
        }

        internal static TableService Instance
        {
            get
            {
                return tableService;
            }
        }

        /// <summary>
        ///  null = not yet attempted to connect
        ///  true = connected
        ///  false = connection failed
        /// </summary>
        internal bool? Connected { get; private set;}

        internal string ConnectionString { get; private set; }

        internal string TableName { get; private set; }

        private CloudTable CloudTable { get; set;}

        internal void Connect()
        {
            if (!this.Connected.HasValue) // if connection not yet attempted
            {
                this.Connected = false; // default

                IEnumerable<AzureTableAppender> azureTableAppenders = LogManager.GetLogger(typeof(AzureTableAppender)).Logger.Repository.GetAppenders().Cast<AzureTableAppender>();

                if (!azureTableAppenders.Any())
                {
                    throw new Exception("Couldn't find a log4net AzureTableAppender");
                }
                else if (azureTableAppenders.Count() > 1)
                {
                    throw new Exception("Found more than one log4net AzureTableAppender");
                }
                else
                {
                    // we have found a single AzureTableAppender
                    AzureTableAppender azureTableAppender = azureTableAppenders.Single();

                    this.ConnectionString = azureTableAppender.ConnectionString;
                    this.TableName = azureTableAppender.TableName;

                    CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(this.ConnectionString);

                    CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

                    this.CloudTable = cloudTableClient.GetTableReference(this.TableName);

                    this.Connected = true;
                }
            }
        }

        internal IEnumerable<SearchItemTableEntity> GetSearchItemTableEntities() // TODO: rename to ReadSearchItemTableEntities
        {
            return this.Connected.HasValue && this.Connected.Value // if connected
                    ? this.CloudTable.ExecuteQuery(new TableQuery<SearchItemTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "searchItem")))
                    : Enumerable.Empty<SearchItemTableEntity>();
        }

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
                            PartitionKey = "searchItem",
                            RowKey = Guid.NewGuid().ToString(),
                            Name = name
                        }));
            }
        }

        internal SearchItemTableEntity GetSearchItemTableEntity(string rowKey) // TODO: rename to ReadSearchItemTableEntity
        {
            return this.Connected.HasValue && this.Connected.Value // if connected
                    ? this.CloudTable
                        .Execute(TableOperation.Retrieve<SearchItemTableEntity>("searchItem", rowKey))
                        .Result as SearchItemTableEntity
                    : null;
        }

        internal void UpdateSearchItemTableEntity(string rowKey, Level minLevel, string hostName, string loggerName)
        {
            this.Connect();
            if (this.Connected.HasValue && this.Connected.Value)
            {
                SearchItemTableEntity searchItemTableEntity = this.GetSearchItemTableEntity(rowKey);
                searchItemTableEntity.MinLevel = minLevel.ToString();
                searchItemTableEntity.HostName = hostName;
                searchItemTableEntity.LoggerName = loggerName;

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
                this.CloudTable.Execute(TableOperation.Delete(new DynamicTableEntity("searchItem", rowKey) { ETag = "*" }));
            }
        }

        /// <summary>
        /// https://azure.microsoft.com/en-gb/documentation/articles/storage-dotnet-how-to-use-tables/
        /// </summary>
        /// <param name="rowKey">(optional) start at the row key after this one</param>
        /// <param name="take">max number of items to return</param>
        /// <returns></returns>
        internal IEnumerable<LogTableEntity> GetLogTableEntities(Level minLevel, string hostName, string loggerName, string rowKey)
        {
            TableQuery<LogTableEntity> tableQuery = new TableQuery<LogTableEntity>();

            if (minLevel != Level.DEBUG)
            {
                switch (minLevel)
                {
                    case Level.INFO: // show all except debug
                        tableQuery.AndWhere(TableQuery.GenerateFilterCondition("Level", QueryComparisons.NotEqual, Level.DEBUG.ToString()));
                        break;

                    case Level.WARN: // show all except debug and info
                        tableQuery.AndWhere(TableQuery.CombineFilters(
                                                TableQuery.GenerateFilterCondition("Level", QueryComparisons.NotEqual, Level.DEBUG.ToString()),
                                                TableOperators.And,
                                                TableQuery.GenerateFilterCondition("Level", QueryComparisons.NotEqual, Level.INFO.ToString())));
                        break;

                    case Level.ERROR: // show if error or fatal
                        tableQuery.AndWhere(TableQuery.CombineFilters(
                                                TableQuery.GenerateFilterCondition("Level", QueryComparisons.Equal, Level.ERROR.ToString()),
                                                TableOperators.Or,
                                                TableQuery.GenerateFilterCondition("Level", QueryComparisons.Equal, Level.FATAL.ToString())));
                        break;

                    case Level.FATAL: // show fatal only
                        tableQuery.AndWhere(TableQuery.GenerateFilterCondition("Level", QueryComparisons.Equal, Level.FATAL.ToString()));

                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(hostName))
            {
                tableQuery.AndWhere(TableQuery.GenerateFilterCondition("log4net_HostName", QueryComparisons.Equal, hostName));
            }

            if (!string.IsNullOrWhiteSpace(loggerName))
            {
                tableQuery.AndWhere(TableQuery.GenerateFilterCondition("LoggerName", QueryComparisons.Equal, loggerName));
            }

            if(!string.IsNullOrWhiteSpace(rowKey))
            {
                tableQuery.AndWhere(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, rowKey));
            }

            return this.Connected.HasValue && this.Connected.Value
                    ? this.CloudTable.ExecuteQuery(tableQuery) // if connected
                    : Enumerable.Empty<LogTableEntity>(); // fallback for not connected
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        internal LogTableEntity GetLogTableEntity(string partitionKey, string rowKey)
        {
            return this.Connected.HasValue && this.Connected.Value // if connected
                    ? this.CloudTable
                        .Execute(TableOperation.Retrieve<LogTableEntity>(partitionKey, rowKey))
                        .Result as LogTableEntity
                    : null;
        }

        ///// <summary>
        ///// WARNING: this will delete all
        ///// </summary>
        //internal void DeleteLog()
        //{
        //    // http://stackoverflow.com/questions/16170915/best-practice-in-deleting-azure-table-entities-in-foreach-loop
        //    // HACK: delete the table - issue puts the table name out of action for a long time
        //    this.CloudTable.Delete();
        //}
    }
}
