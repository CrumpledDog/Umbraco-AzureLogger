namespace UmbracoAzureLogger.Core.Models
{
    // this is the state of all filters that can be saved (and searched on in Azure)
    public class SearchFiltersState
    {
        public Level MinLevel { get; set; }

        public string HostName { get; set; }

        public string LoggerName { get; set; }
    }
}
