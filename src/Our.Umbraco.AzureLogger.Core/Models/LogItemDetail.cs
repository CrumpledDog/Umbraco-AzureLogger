namespace Our.Umbraco.AzureLogger.Core.Models
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// A detailed POCO for a LogTableEntity
    /// allows full control over serialization (as TableEntity base class is external)
    /// </summary>
    public class LogItemDetail
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("identity")]
        public string Identity { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("threadName")]
        public string ThreadName { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("processId")]
        public int ProcessId { get; set; }

        [JsonProperty("appDomainId")]
        public int AppDomainId { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
