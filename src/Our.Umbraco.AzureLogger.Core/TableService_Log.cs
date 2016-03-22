namespace Our.Umbraco.AzureLogger.Core
{
    using Extensions;
    using log4net.Core;
    using Microsoft.WindowsAzure.Storage.Table;
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Level = Our.Umbraco.AzureLogger.Core.Models.Level;

    internal sealed partial class TableService
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="loggingEvents">collection of log4net logging events to persist into Azure table storage</param>
        internal void CreateLogTableEntities(LoggingEvent[] loggingEvents)
        {
            this.Connect();
            if (this.Connected.HasValue && this.Connected.Value)
            {
                // group by logging event date - each group equates to an Azure table partition key
                // (all items in the same Azure table batch operation must use the same partition key)
                foreach(IGrouping<DateTime, LoggingEvent> groupedLoggingEvents in loggingEvents.GroupBy(x => x.TimeStamp.Date))
                {
                    DateTime date = groupedLoggingEvents.Key; // date for current grouping

                    // set partition key for this batch of inserts
                    string partitionKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - date.AddDays(date.Day).Ticks + 1);

                    // ensure 100 or less items are inserted per Azure table batch insert operation
                    foreach(IEnumerable<LoggingEvent> batchLoggingEvents in groupedLoggingEvents.Batch(100))
                    {
                        TableBatchOperation tableBatchOperation = new TableBatchOperation();

                        foreach(LoggingEvent loggingEvent in batchLoggingEvents)
                        {
                            tableBatchOperation.Insert(new LogTableEntity(partitionKey, loggingEvent));
                        }

                        this.CloudTable.ExecuteBatch(tableBatchOperation);
                    }
                }
            }
        }

        /// <summary>
        /// https://azure.microsoft.com/en-gb/documentation/articles/storage-dotnet-how-to-use-tables/
        /// </summary>
        /// <param name="rowKey">(optional) start at the row key after this one</param>
        /// <param name="take">max number of items to return</param>
        /// <returns></returns>
        internal IEnumerable<LogTableEntity> ReadLogTableEntities(string partitionKey, string rowKey, Level minLevel, string hostName, bool loggerNamesInclude, string[] loggerNames)
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

            //tableQuery.AndWhere(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, TableService.SearchItemPartitionKey));

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

            if (loggerNames.Any())
            {
                string queryComparison = loggerNamesInclude ? QueryComparisons.Equal : QueryComparisons.NotEqual;
                string tableOperator = loggerNamesInclude ? TableOperators.Or : TableOperators.And;

                // full nested clause of filters
                string loggerNamesFilter;

                // each filter
                List<string> loggerNameFilters = new List<string>();

                foreach (string loggerName in loggerNames)
                {
                    loggerNameFilters.Add(TableQuery.GenerateFilterCondition("LoggerName", queryComparison, loggerName));
                }

                loggerNamesFilter = loggerNameFilters.First();

                foreach (string loggerNameFilter in loggerNameFilters.Skip(1))
                {
                    loggerNamesFilter += " " + tableOperator + " " + loggerNameFilter;
                }

                tableQuery.AndWhere(loggerNamesFilter);
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
        internal LogTableEntity ReadLogTableEntity(string partitionKey, string rowKey)
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
