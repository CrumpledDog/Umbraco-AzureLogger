namespace Our.Umbraco.AzureLogger.Core
{
    using System;

    /// <summary>
    /// Thrown when an excess number of Azure table calls have been made for a request
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
        /// any data from the query that has already been found
        /// </summary>
        internal new object[] Data { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="lastPartitionKey"></param>
        /// <param name="lastRowKey"></param>
        /// <param name="data"></param>
        internal TableQueryTimeoutException(string lastPartitionKey, string lastRowKey, object[] data)
        {
            this.LastPartitionKey = lastPartitionKey;
            this.LastRowKey = lastRowKey;
            this.Data = data;
        }
    }
}
