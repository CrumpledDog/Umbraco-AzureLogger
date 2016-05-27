namespace Our.Umbraco.AzureLogger.Installer
{
    using System;
    using System.Configuration;
    public static class Helpers
    {
        public static Version GetUmbracoVersion()
        {
            var umbracoVersion = new Version(ConfigurationManager.AppSettings["umbracoConfigurationStatus"]);
            return umbracoVersion;
        }
    }
}
