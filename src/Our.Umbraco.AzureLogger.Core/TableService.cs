namespace Our.Umbraco.AzureLogger.Core
{
    using Extensions;
    using log4net;
    using log4net.Core;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Level = Our.Umbraco.AzureLogger.Core.Models.Level;

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
        /// Singleton constructor
        /// </summary>
        private TableService()
        {
        }

        /// <summary>
        /// Get a reference to the singleton instance of the TableService
        /// </summary>
        internal static TableService Instance
        {
            get
            {
                return tableService;
            }
        }

        /// <summary>
        /// Persists log4net logging events into Azure table storage
        /// </summary>
        /// <param name="appenderName"></param>
        /// <param name="loggingEvents">collection of log4net logging events to persist into Azure table storage</param>
        internal void CreateLogTableEntities(string appenderName, LoggingEvent[] loggingEvents)
        {
            CloudTable cloudTable = this.GetCloudTable(appenderName);

            if (cloudTable != null)
            {
                // all loggingEvents converted to LogTableEntity objects (required for indexing method - avoids duplicate constructions)
                List<LogTableEntity> logTableEntities = new List<LogTableEntity>();

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
                            // logic in constructor also parses out dictionary items into properties
                            LogTableEntity logTableEntity = new LogTableEntity(partitionKey, loggingEvent);

                            // add to collection for indexing later
                            logTableEntities.Add(logTableEntity);

                            tableBatchOperation.Insert(logTableEntity);
                        }

                        cloudTable.ExecuteBatch(tableBatchOperation);
                    }
                }

                IndexService.Instance.Process(appenderName, logTableEntities);
            }
        }

        /// <summary>
        /// Wrapper method to handle any filtering (Azure table queries can't do wild card matching)
        /// </summary>
        /// <param name="appenderName"></param>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <param name="hostName"></param>
        /// <param name="loggerName"></param>
        /// <param name="messageIntro"></param>
        /// <returns></returns>
        internal IEnumerable<LogTableEntity> ReadLogTableEntities(string appenderName, string partitionKey, string rowKey, string hostName, string loggerName, Level minLevel, string message, string sessionId, int take)
        {
            // flags to indiciate if filtering is requied here in c#
            bool hostNameWildcardFiltering = !string.IsNullOrWhiteSpace(hostName) && !IndexService.Instance.GetMachineNames(appenderName).Any(x => x == hostName);
            bool loggerNameWildcardFiltering = !string.IsNullOrWhiteSpace(loggerName) && !IndexService.Instance.GetLoggerNames(appenderName).Any(x => x == loggerName);

            // check to see if any filtering needs to be done here (always filter message as it has no index)
            if (!string.IsNullOrWhiteSpace(message) || hostNameWildcardFiltering || loggerNameWildcardFiltering)
            {
                // additional filtering here- may require additional azure table queries

                List<LogTableEntity> logTableEntities = new List<LogTableEntity>(); // collection to return

                // calculate excess amount to request from a cloud query, which will filter down to the required take
                // machine name: low
                // logger name: low
                // level-debug: low
                // level-info: medium
                // level-warn: high
                // level-error: high
                // level-fatal: high
                // message: medium

                int lastCount;
                string lastPartitionKey = partitionKey;
                string lastRowKey = rowKey;
                int attempts = 0;
                bool finished = false;

                do
                {
                    attempts++;

                    if (attempts >= 3)
                    {
                        throw new TableQueryTimeoutException(lastPartitionKey, lastRowKey, logTableEntities.ToArray());
                    }

                    lastCount = logTableEntities.Count;

                    // take a large chunk to filter here - take size should be relative to the filter granularity
                    IEnumerable<LogTableEntity> returnedLogTableEntities = this.ReadLogTableEntities(
                                                                                    appenderName,
                                                                                    lastPartitionKey,
                                                                                    lastRowKey,
                                                                                    minLevel,
                                                                                    loggerNameWildcardFiltering ? null : loggerName, // only set if
                                                                                    hostNameWildcardFiltering ? null : hostName,
                                                                                    sessionId) // not using wildcards
                                                                                .Take(100);

                    if (returnedLogTableEntities.Any())
                    {
                        logTableEntities.AddRange(returnedLogTableEntities);

                        // set last known, before filtering out
                        lastPartitionKey = logTableEntities.Last().PartitionKey;
                        lastRowKey = logTableEntities.Last().RowKey;

                        // performing filtering on local list, otherwise it seems to affect table query performance (as every row in table returned and cast)
                        logTableEntities = logTableEntities
                                        .Where(x => string.IsNullOrWhiteSpace(hostName) || (x.log4net_HostName != null && x.log4net_HostName.IndexOf(hostName, StringComparison.InvariantCultureIgnoreCase) > -1))
                                        .Where(x => string.IsNullOrWhiteSpace(loggerName) || (x.LoggerName != null && x.LoggerName.IndexOf(loggerName, StringComparison.InvariantCultureIgnoreCase) > -1))
                                        .Where(x => string.IsNullOrWhiteSpace(message) || (x.Message != null && x.Message.IndexOf(message, StringComparison.InvariantCultureIgnoreCase) > -1))
                                        .ToList();
                    }
                    else
                    {
                        // no data returned from Azure query
                        finished = true;
                    }

                }
                while (logTableEntities.Count < take && !finished);

                return logTableEntities.Take(take); // trim any excess
            }
            else
            {
                // filter at azure table query level only
                return this.ReadLogTableEntities(appenderName, partitionKey, rowKey, minLevel, loggerName, hostName, sessionId).Take(take);
            }
        }

        /// <summary>
        /// Attempts to perform an Azure table query
        /// https://azure.microsoft.com/en-gb/documentation/articles/storage-dotnet-how-to-use-tables/
        /// Gets a collection of LogTableEntity objs suitable for casting to LogItemInto
        /// </summary>
        /// <param name="appenderName"></param>
        /// <param name="partitionKey">null or the last known partition key</param>
        /// <param name="rowKey">null or the last known row key</param>
        /// <param name="minLevel"></param>
        /// <param name="loggerName">if set, looks for an exact match</param>
        /// <param name="hostName">if set, looks for an exact match</param>
        /// <returns>a collection of log items matching the supplied filter criteria</returns>
        private IEnumerable<LogTableEntity> ReadLogTableEntities(string appenderName, string partitionKey, string rowKey, Level minLevel, string loggerName, string hostName, string sessionId)
        {
            CloudTable cloudTable = this.GetCloudTable(appenderName);

            if (cloudTable != null)
            {
                TableQuery<LogTableEntity> tableQuery = new TableQuery<LogTableEntity>()
                                                                .Select(new string[] {  // reduce data fields returned from Azure
                                                                    "Level",
                                                                    "LoggerName",
                                                                    "Message",
                                                                    "EventTimeStamp",
                                                                    "log4net_HostName"
                                                                });

                if (!string.IsNullOrWhiteSpace(partitionKey))
                {
                    tableQuery.AndWhere(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, partitionKey));
                }

                if (!string.IsNullOrWhiteSpace(rowKey))
                {
                    tableQuery.AndWhere(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, rowKey));
                }

                if (minLevel != Level.DEBUG)
                {
                    // a number comparrison would be better, but log4net level and enum level don't match
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

                if (!string.IsNullOrWhiteSpace(loggerName))
                {
                    tableQuery.AndWhere(TableQuery.GenerateFilterCondition("LoggerName", QueryComparisons.Equal, loggerName));
                }
                else
                {
                    // HACK: ensure index entities are not returned
                    tableQuery.AndWhere(TableQuery.GenerateFilterCondition("LoggerName", QueryComparisons.NotEqual, string.Empty));
                }

                if (!string.IsNullOrWhiteSpace(hostName))
                {
                    tableQuery.AndWhere(TableQuery.GenerateFilterCondition("log4net_HostName", QueryComparisons.Equal, hostName));
                }

                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    tableQuery.AndWhere(TableQuery.GenerateFilterCondition("sessionId", QueryComparisons.Equal, sessionId));
                }

                return cloudTable.ExecuteQuery(
                    tableQuery,
                    new TableRequestOptions() {
                        ServerTimeout = new TimeSpan(0,0,2)
                    });
            }

            return Enumerable.Empty<LogTableEntity>(); // fallback
        }

        /// <summary>
        /// Returns a specific log entry, with all data required to inflate a <see cref="LogItemDetail"/>
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

        /// <summary>
        /// Attempts to delete the cloud table
        /// </summary>
        /// <param name="appenderName"></param>
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
        /// Creates IndexTableEntity objects to be persisted in Azure table storage
        /// </summary>
        /// <param name="appenderName"></param>
        /// <param name="partitionKey">the index name</param>
        /// <param name="rowKeys">an index value</param>
        internal void CreateIndexTableEntities(string appenderName, string partitionKey, string[] rowKeys)
        {
            CloudTable cloudTable = this.GetCloudTable(appenderName);

            if (cloudTable != null)
            {
                // ensure 100 or less items are inserted per Azure table batch insert operation
                foreach (IEnumerable<string> batchRowKeys in rowKeys.Batch(100))
                {
                    TableBatchOperation tableBatchOperation = new TableBatchOperation();

                    foreach(string rowKey in batchRowKeys)
                    {
                        tableBatchOperation.Insert(new IndexTableEntity(partitionKey, rowKey));
                    }

                    try
                    {
                        cloudTable.ExecuteBatch(tableBatchOperation);
                    }
                    catch
                    {
                        // surpress any errors (as another server may have already updated some or all of these index values)
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="appenderName"></param>
        /// <param name="partitionKey">index name, eg. host_name</param>
        /// <returns></returns>
        internal IEnumerable<IndexTableEntity> ReadIndexTableEntities(string appenderName, string partitionKey)
        {
            CloudTable cloudTable = this.GetCloudTable(appenderName);

            if (cloudTable != null)
            {
                TableQuery<IndexTableEntity> tableQuery = new TableQuery<IndexTableEntity>();

                tableQuery.AndWhere(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

                return cloudTable.ExecuteQuery(tableQuery);
            }

            return Enumerable.Empty<IndexTableEntity>(); // fallback
        }

        /// <summary>
        /// Helper to get the cloud table associated with the supplied appender name
        /// </summary>
        /// <param name="appenderName">unique name to identify a log4net Azure TableAppender</param>
        /// <returns></returns>
        internal CloudTable GetCloudTable(string appenderName)
        {
            if (!this.appenderCloudTables.ContainsKey(appenderName))
            {
                // attempt to get at table from the appender details
                TableAppender tableAppender = this.GetTableAppender(appenderName);

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

                    //bool retry;
                    //do
                    //{
                    //    retry = false;
                        try
                        {
                            cloudTable.CreateIfNotExists();

                            cloudTableReady = true;
                        }
                        catch //(StorageException exception)
                        {
                            //if (exception.RequestInformation.HttpStatusCode == 409 &&
                            //    exception.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableBeingDeleted))
                            //{
                            //    retry = true;
                            //}
                        }
                    //} while (retry);

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

        internal TableAppender GetTableAppender(string appenderName)
        {
            return  this.GetTableAppenders().SingleOrDefault(x => x.Name == appenderName);
        }

        internal IEnumerable<TableAppender> GetTableAppenders()
        {
            return LogManager
                    .GetLogger(typeof(TableAppender))
                    .Logger
                    .Repository
                    .GetAppenders()
                    .Where(x => x is TableAppender)
                    .Cast<TableAppender>()
                    .OrderBy(x => x.Name);
        }
    }
}
