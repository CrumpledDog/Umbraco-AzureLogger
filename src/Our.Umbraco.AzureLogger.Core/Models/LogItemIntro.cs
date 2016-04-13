namespace Our.Umbraco.AzureLogger.Core.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System;

    /// <summary>
    /// A lightweight POCO for a LogTableEntity
    /// allows full control over serialization (as TableEntity base class is external)
    /// </summary>
    public class LogItemIntro
    {
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("rowKey")]
        public string RowKey { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("level")]
        public Level Level { get; set; }

        [JsonProperty("loggerName")]
        public string LoggerName { get; set; }

        /// <summary>
        /// The full message could be a lot of data, so this is only a portion of the full message
        /// </summary>
        [JsonProperty("messageIntro")]
        public string MessageIntro { get; set; }

        [JsonProperty("eventTimestamp")]
        public DateTime EventTimestamp { get; set; }

        [JsonProperty("hostName")]
        public string HostName { get; set; }
    }
}
