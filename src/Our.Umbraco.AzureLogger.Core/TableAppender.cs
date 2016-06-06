namespace Our.Umbraco.AzureLogger.Core
{
    using log4net.Appender;
    using log4net.Core;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Threading;
    using System.Threading.Tasks;
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
        /// From (optional) configuration setting
        /// </summary>
        public string TreeName { get; set; }

        /// <summary>
        /// From (optional) configuration setting
        /// </summary>
        public string IconName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TableAppender()
        {
            this.Lossy = false;
        }

        /// <summary>
        /// Attempts to get the associated azure table, but gives up if it takes longer than 1/2 second
        /// </summary>
        /// <returns>true if the appender can connect to table storage, otherwise false</returns>
        internal bool IsConnected()
        {
            bool isConnected = false;

            Thread thread = new Thread(() => {
                CloudTable cloudTable = TableService.Instance.GetCloudTable(this.Name);
                isConnected = cloudTable != null && cloudTable.Exists();//( new TableRequestOptions() { ServerTimeout = new System.TimeSpan(500) });
            });

            thread.Start();

            if (!thread.Join(500)) { thread.Abort();}

            return isConnected;
        }

        /// <summary>
        /// Append extra logging data to a log item
        /// </summary>
        /// <param name="loggingEvent">the log item to extend</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            loggingEvent.Properties["url"] = null;
            loggingEvent.Properties["sessionId"] = null;

            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Handler != null)
                {
                    if (HttpContext.Current.Request != null)
                    {
                        loggingEvent.Properties["url"] = HttpContext.Current.Request.RawUrl;
                    }

                    if (HttpContext.Current.Session != null)
                    {
                        loggingEvent.Properties["sessionId"] = HttpContext.Current.Session.SessionID;
                    }
                }
            }
            catch
            {
                // failsafe as no exceptions should be ever thrown in this method
            }

            base.Append(loggingEvent);
        }

        /// <summary>
        /// log4net calls this to persist the logging events
        /// </summary>
        /// <param name="events">the log events to persist</param>
        protected override void SendBuffer(LoggingEvent[] loggingEvents)
        {
            // spin off a new thread to avoid waiting
            Task.Run(() => TableService.Instance.CreateLogTableEntities(this.Name, loggingEvents));
        }
    }
}
