namespace Our.Umbraco.AzureLogger.Core
{
    using log4net.Appender;
    using log4net.Core;
    using System.Web;

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
        /// From configuration setting
        /// </summary>
        public string TreeName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TableAppender()
        {
            // TODO: read this from a config value
            this.Lossy = false;
        }

        /// <summary>
        /// Append extra logging data
        /// </summary>
        /// <param name="loggingEvent"></param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Handler != null)
                {
                    loggingEvent.Properties["url"] = HttpContext.Current.Request.Url.AbsoluteUri;
                }
                else
                {
                    loggingEvent.Properties["url"] = null;
                }
            }
            catch
            {
                loggingEvent.Properties["url"] = null;
            }

            base.Append(loggingEvent);
        }

        /// <summary>
        /// log4net calls this to persist the logging events
        /// </summary>
        /// <param name="events">the log events to persist</param>
        protected override void SendBuffer(LoggingEvent[] loggingEvents)
        {
            TableService.Instance.CreateLogTableEntities(this.Name, loggingEvents);
        }
    }
}
