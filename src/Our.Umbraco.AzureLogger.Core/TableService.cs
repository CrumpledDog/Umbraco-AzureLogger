namespace Our.Umbraco.AzureLogger.Core
{
    using Extensions;
    using log4net;
    using log4net.Core;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Microsoft.WindowsAzure.Storage.Table.Protocol;
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    internal sealed class TableService
    {
        private static readonly TableService tableService = new TableService();

        /// <summary>
        /// collection of CloudTable objs, each associated with a unique log4net appender
        /// </summary>
        private ConcurrentDictionary<string, CloudTable> appenderCloudTables = new ConcurrentDictionary<string, CloudTable>();

        static TableService()
        {
        }

        /// <summary>
        /// prevent external construction (as is singleton)
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
        ///
        /// </summary>
        /// <param name="appenderName"></param>
        /// <param name="loggingEvents">collection of log4net logging events to persist into Azure table storage</param>
        internal void CreateLogTableEntities(string appenderName, LoggingEvent[] loggingEvents)
        {
            CloudTable cloudTable = this.GetCloudTable(appenderName);

            if (cloudTable != null)
            {
                // group by logging event date & hour - each group equates to an Azure table partition key
                // (all items in the same Azure table batch operation must use the same partition key)
                foreach (IGrouping<DateTime, LoggingEvent> groupedLoggingEvents in loggingEvents.GroupBy(x => x.TimeStamp.Date.AddHours(x.TimeStamp.Hour)))
                {
                    DateTime dateHour = groupedLoggingEvents.Key; // date for current grouping

                    // set partition key for this batch of inserts
                    string partitionKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - dateHour.Ticks + 1);

                    // ensure 100 or less items are inserted per Azure table batch insert operation
                    foreach (IEnumerable<LoggingEvent> batchLoggingEvents in groupedLoggingEvents.Batch(100))
                    {
                        TableBatchOperation tableBatchOperation = new TableBatchOperation();

                        foreach (LoggingEvent loggingEvent in batchLoggingEvents)
                        {
                            tableBatchOperation.Insert(new LogTableEntity(partitionKey, loggingEvent));
                        }

                        cloudTable.ExecuteBatch(tableBatchOperation);
                    }
                }
            }
        }

        /// <summary>
        /// https://azure.microsoft.com/en-gb/documentation/articles/storage-dotnet-how-to-use-tables/
        /// </summary>
        /// <param name="partitionKey">null or the last known partition key</param>
        /// <param name="rowKey">null or the last known row key</param>
        /// <param name="minLevel">the min level of log items to return</param>
        /// <param name="hostName">null or the hostname to filter on</param>
        /// <param name="loggerNamesInclude">indicates whether the loggerNames should be included or excluded in the filter query</param>
        /// <param name="loggerNames">array of logger names</param>
        /// <returns>a collection of log items matchin the supplied filter criteria</returns>
        internal IEnumerable<LogTableEntity> ReadLogTableEntities(string appenderName, string partitionKey, string rowKey)
        {
            CloudTable cloudTable = this.GetCloudTable(appenderName);

            if (cloudTable != null)
            {
                TableQuery<LogTableEntity> tableQuery = new TableQuery<LogTableEntity>();

                if (!string.IsNullOrWhiteSpace(partitionKey))
                {
                    tableQuery.AndWhere(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, partitionKey));
                }

                if (!string.IsNullOrWhiteSpace(rowKey))
                {
                    tableQuery.AndWhere(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, rowKey));
                }

                return cloudTable.ExecuteQuery(tableQuery);
            }

            return Enumerable.Empty<LogTableEntity>(); // fallback
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        internal LogTableEntity ReadLogTableEntity(string appenderName, string partitionKey, string rowKey)
        {
            CloudTable cloudTable = this.GetCloudTable(appenderName);

            if (cloudTable != null)
            {
                return cloudTable
                        .Execute(TableOperation.Retrieve<LogTableEntity>(partitionKey, rowKey))
                        .Result as LogTableEntity;
            }

            return null; // fallback
        }

        internal void WipeLog(string appenderName)
        {
            CloudTable cloudTable;

            // if table found in collection, then delete it
            if (this.appenderCloudTables.TryRemove(appenderName, out cloudTable))
            {
                cloudTable.Delete();
            }
        }

        /// <summary>
        /// Helper to get the cloud table associated with the supplied appender name
        /// </summary>
        /// <param name="appenderName">unique name to identify a log4net Azure TableAppender</param>
        /// <returns></returns>
        private CloudTable GetCloudTable(string appenderName)
        {
            if (!this.appenderCloudTables.ContainsKey(appenderName))
            {
                // attempt to get at table from the appender details
                TableAppender tableAppender = LogManager
                                                .GetLogger(typeof(TableAppender))
                                                .Logger
                                                .Repository
                                                .GetAppenders()
                                                .Where(x => x is TableAppender)
                                                .Cast<TableAppender>()
                                                .SingleOrDefault(x => x.Name == appenderName);

                if (tableAppender != null)
                {
                    string connectionString;

                    // attempt to find connection string in web.config
                    if (ConfigurationManager.ConnectionStrings[tableAppender.ConnectionString] != null)
                    {
                        connectionString = ConfigurationManager.ConnectionStrings[tableAppender.ConnectionString].ConnectionString;
                    }
                    else
                    {
                        // fallback to assuming tableAppender has the full connection string
                        connectionString = tableAppender.ConnectionString;
                    }

                    CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);

                    CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

                    CloudTable cloudTable = cloudTableClient.GetTableReference(tableAppender.TableName);

                    bool cloudTableReady = false;

                    //do
                    //{
                        try
                        {
                            cloudTable.CreateIfNotExists();

                            cloudTableReady = true;
                        }
                        catch (StorageException exception)
                        {
                            //if (exception.RequestInformation.HttpStatusCode == 409 &&
                            //    exception.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableBeingDeleted))
                            //{
                            //    // TODO: pause and try again ?
                            //}
                        }
                    //} while (!cloudTableReady);

                    if (cloudTableReady)
                    {
                        this.appenderCloudTables[appenderName] = cloudTable;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return this.appenderCloudTables[appenderName];
        }
    }
}
