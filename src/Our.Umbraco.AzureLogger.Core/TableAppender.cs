namespace Our.Umbraco.AzureLogger.Core
{
    using log4net.Appender;
    using log4net.Core;

    /// <summary>
    /// Custom log4net appender
    /// </summary>
    public class TableAppender : BufferingAppenderSkeleton
    {
        /// <summary>
        /// From configuration setting
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// From configuration setting
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="events"></param>
        protected override void SendBuffer(LoggingEvent[] events)
        {
            // NOTE: Azure Table Storage can handle batch inserts of up to 100 items




        }
    }
}
