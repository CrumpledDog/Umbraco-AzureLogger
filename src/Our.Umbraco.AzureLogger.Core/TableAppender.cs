namespace Our.Umbraco.AzureLogger.Core
{
    using log4net.Appender;
    using log4net.Core;

    /// <summary>
    /// Custom log4net appender
    /// </summary>
    public class TableAppender : BufferingAppenderSkeleton
    {
        public string ConnectionString { get; set; }

        public string TableName { get; set; }

        protected override void SendBuffer(LoggingEvent[] events)
        {
        }
    }
}
