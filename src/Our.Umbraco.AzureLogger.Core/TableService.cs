namespace Our.Umbraco.AzureLogger.Core
{
    using Extensions;
    using log4net;
    using log4net.Core;
    using Microsoft.WindowsAzure.Storage.Table;
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Level = Our.Umbraco.AzureLogger.Core.Models.Level;

    /// <summary>
    /// Singleton service class
    /// </summary>
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
        /// <param name="appenderName">name of the log4net appender</param>
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
        /// Queries Azure table storage until results are returned or a timeout is thown
        /// </summary>
        /// <param name="appenderName">name of the log4net appender</param>
        /// <param name="partitionKey">a partional key to begin search from (can be null)</param>
        /// <param name="rowKey">a row key to begin search from (can be null)</param>
        /// <param name="hostName">host name to filter on</param>
        /// <param name="loggerName">logger name to filter on</param>
        /// <param name="minLevel">logger level to filter</param>
        /// <param name="message">message text to filter</param>
        /// <param name="sessionId">session id to filter</param>
        /// <returns></returns>
        internal LogTableEntity[] ReadLogTableEntities(string appenderName, string partitionKey, string rowKey, string hostName, string loggerName, Level minLevel, string message, string sessionId)
        {
            LogTableEntity[] logTableEntities = new LogTableEntity[] { }; // default return value

            CloudTable cloudTable = this.GetCloudTable(appenderName);

            if (cloudTable == null)
            {
                return logTableEntities;
            }

            bool hostNameWildcardFiltering = !string.IsNullOrWhiteSpace(hostName) && !IndexService.Instance.GetMachineNames(appenderName).Any(x => x == hostName);
            bool loggerNameWildcardFiltering = !string.IsNullOrWhiteSpace(loggerName) && !IndexService.Instance.GetLoggerNames(appenderName).Any(x => x == loggerName);

            // local filtering function applied to returned Azure table results
            Func<LogTableEntity, bool> customFiltering = (x) => { return true; }; // default empty method (no custom filtering performed)

            // check to see if custom filtering (in c#) is required in addition to the Azure query
            if (hostNameWildcardFiltering || loggerNameWildcardFiltering || !string.IsNullOrWhiteSpace(message)) // message filtering always done in c#
            {
                customFiltering = (x) =>
                {
                    return (string.IsNullOrWhiteSpace(hostName) || x.log4net_HostName != null && x.log4net_HostName.IndexOf(hostName, StringComparison.InvariantCultureIgnoreCase) > -1)
                        && (string.IsNullOrWhiteSpace(loggerName) || x.LoggerName != null && x.LoggerName.IndexOf(loggerName, StringComparison.InvariantCultureIgnoreCase) > -1)
                        && (string.IsNullOrWhiteSpace(message) || x.Message != null && x.Message.IndexOf(message, StringComparison.InvariantCultureIgnoreCase) > -1);
                };
            }

            // build the Azure table query
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

            if (!loggerNameWildcardFiltering && !string.IsNullOrWhiteSpace(loggerName))
            {
                tableQuery.AndWhere(TableQuery.GenerateFilterCondition("LoggerName", QueryComparisons.Equal, loggerName));
            }
            else
            {
                // HACK: ensure index entities are not returned
                tableQuery.AndWhere(TableQuery.GenerateFilterCondition("LoggerName", QueryComparisons.NotEqual, string.Empty));
            }

            if (!hostNameWildcardFiltering && !string.IsNullOrWhiteSpace(hostName))
            {
                tableQuery.AndWhere(TableQuery.GenerateFilterCondition("log4net_HostName", QueryComparisons.Equal, hostName));
            }

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                tableQuery.AndWhere(TableQuery.GenerateFilterCondition("sessionId", QueryComparisons.Equal, sessionId));
            }

            tableQuery.Take(50); // max results to be returned in single query

            // perform query
            TableContinuationToken tableContinuationToken = null;
            TableQuerySegment<LogTableEntity> response;
            bool retry;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                retry = false;

                response = cloudTable.ExecuteQuerySegmented(tableQuery, tableContinuationToken); // blocking

                logTableEntities = response.Results.Where(x => customFiltering(x)).ToArray();

                if (!logTableEntities.Any() && tableContinuationToken != null)
                {
                    if (stopwatch.ElapsedMilliseconds > 10000) // 10 seconds
                    {
                        throw new TableQueryTimeoutException(response.ContinuationToken.NextPartitionKey, response.ContinuationToken.NextRowKey);
                    }

                    tableContinuationToken = response.ContinuationToken;

                    retry = true;
                }

            } while (retry);


            return logTableEntities;
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
        /// <param name="appenderName">name of the log4net appender</param>
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
        /// <param name="appenderName">name of the log4net appender</param>
        /// <param name="partitionKey">the index name</param>
        /// <param name="rowKeys">index values</param>
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
        /// Queries Azure table storage for all index table entities of a given index name
        /// </summary>
        /// <param name="appenderName">name of the log4net appender</param>
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
        /// Helper to get (and cache) the cloud table associated with the supplied appender name
        /// </summary>
        /// <param name="appenderName">name of the log4net appender</param>
        /// <returns>the CloudTable for the supplied appender if found, otherwise null</returns>
        private CloudTable GetCloudTable(string appenderName)
        {
            // if not in local cache
            if (!this.appenderCloudTables.ContainsKey(appenderName))
            {
                TableAppender tableAppender = this.GetTableAppender(appenderName);

                if (tableAppender != null)
                {
                    CloudTable cloudTable = tableAppender.GetCloudTable();

                    if (cloudTable != null)
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

        /// <summary>
        /// Get a specific TableAppender
        /// </summary>
        /// <param name="appenderName">name of the log4net appender</param>
        /// <returns>the TableAppender or null</returns>
        internal TableAppender GetTableAppender(string appenderName)
        {
            return  this.GetTableAppenders().SingleOrDefault(x => x.Name == appenderName);
        }

        /// <summary>
        /// Get all TableAppenders
        /// </summary>
        /// <returns>an eumerable of all TableAppenders</returns>
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
