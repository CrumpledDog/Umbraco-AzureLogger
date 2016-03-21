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

        public TableAppender()
        {
            this.Lossy = false;

            // TODO: make this a config parameter
            this.BufferSize = 9; // a value of 100 will buffer 101 items as zero indexed
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
        ///
        /// </summary>
        /// <param name="events"></param>
        protected override void SendBuffer(LoggingEvent[] loggingEvents)
        {
            // NOTE: Azure Table Storage can handle batch inserts of up to 100 items

            TableService.Instance.CreateLogTableEntities(loggingEvents);
        }
    }
}
