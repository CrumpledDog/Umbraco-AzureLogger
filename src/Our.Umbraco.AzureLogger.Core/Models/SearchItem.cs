namespace Our.Umbraco.AzureLogger.Core.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// POCO for a SeachItemTableEntity
    /// allows full control over serialization (as TableEntity base class is external)
    /// </summary>
    public class SearchItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("minLevel")]
        public Level MinLevel { get; set; }

        [JsonProperty("hostName")]
        public string HostName { get; set; }

        [JsonProperty("loggerName")]
        public string LoggerName { get; set; }
    }
}
