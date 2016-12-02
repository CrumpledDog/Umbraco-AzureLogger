namespace Our.Umbraco.AzureLogger.Core
{
    using System;

    /// <summary>
    /// Thrown after queries to Azure table storage return have returned no results within a set time period
    /// </summary>
    internal class TableQueryTimeoutException : Exception
    {
        /// <summary>
        /// the partition key of the last row checked
        /// </summary>
        internal string LastPartitionKey { get; private set; }

        /// <summary>
        /// the row key of the last row checked
        /// </summary>
        internal string LastRowKey { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="lastPartitionKey">the partition key of the last row checked</param>
        /// <param name="lastRowKey">the row key of the last row checked</param>
        internal TableQueryTimeoutException(string lastPartitionKey, string lastRowKey)
        {
            this.LastPartitionKey = lastPartitionKey;
            this.LastRowKey = lastRowKey;
        }
    }
}
