namespace Our.Umbraco.AzureLogger.Core
{
    using System;

    // TODO: can this be internal ?
    public class TableQueryTimeoutException : Exception
    {
        /// <summary>
        /// the partition key of the last row checked
        /// </summary>
        public string LastPartitionKey { get; private set; }

        /// <summary>
        /// the row key of the last row checked
        /// </summary>
        public string LastRowKey { get; private set; }

        /// <summary>
        /// any data from the query that has already been found
        /// </summary>
        public object[] Data { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="lastPartitionKey"></param>
        /// <param name="lastRowKey"></param>
        /// <param name="data"></param>
        public TableQueryTimeoutException(string lastPartitionKey, string lastRowKey, object[] data)
        {
            this.LastPartitionKey = lastPartitionKey;
            this.LastRowKey = lastRowKey;
            this.Data = data;
        }
    }
}
