namespace Our.Umbraco.AzureLogger.Installer.Enums
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstallerStatus
    {
        Ok,
        SaveXdtError,
        SaveConfigError,
        ConnectionError
    }
}
