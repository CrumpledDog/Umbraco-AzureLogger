namespace Our.Umbraco.AzureLogger.Core.Models.TableEntities
{
    using log4net.Core;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Collections;
    using Level = Our.Umbraco.AzureLogger.Core.Models.Level;

    /// <summary>
    /// POCO to match the field names in the Azure table (NOTE: casing of properties matches the Azure field names)
    /// </summary>
    public class LogTableEntity : TableEntity
    {
        /// <summary>
        /// Default Constructor, as required by the Azure storage api
        /// </summary>
        public LogTableEntity()
        {
        }

        /// <summary>
        /// Constructor to create a LogTableEntity from a log4net LoggingEvent
        /// </summary>
        /// <param name="partitionKey">the Azure Table partition key to use</param>
        /// <param name="loggingEvent">the log4net LoggingEvent object</param>
        internal LogTableEntity(string partitionKey, LoggingEvent loggingEvent)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = string.Format("{0:D19}.{1}", DateTime.MaxValue.Ticks - loggingEvent.TimeStamp.Ticks, Guid.NewGuid().ToString().ToLower());
            this.Domain = loggingEvent.Domain;
            this.Identity = loggingEvent.Identity;
            this.Level = loggingEvent.Level.ToString();
            this.LoggerName = loggingEvent.LoggerName;
            this.Message = loggingEvent.RenderedMessage + Environment.NewLine + loggingEvent.GetExceptionString();
            this.EventTimeStamp = loggingEvent.TimeStamp;
            this.ThreadName = loggingEvent.ThreadName;
            this.UserName = loggingEvent.UserName;
            this.Location = loggingEvent.LocationInformation.FullInfo;

            foreach(DictionaryEntry dictionaryEntry in loggingEvent.Properties)
            {
                switch (dictionaryEntry.Key.ToString())
                {
                    case "processId":
                        this.processId = (int)dictionaryEntry.Value;
                        break;

                    case "appDomainId":
                        this.appDomainId = (int)dictionaryEntry.Value;
                        break;

                    case "log4net:HostName":
                        this.log4net_HostName = (string)dictionaryEntry.Value;
                        break;

                    case "url":
                        this.url = (string)dictionaryEntry.Value;
                        break;

                    case "sessionId":
                        this.sessionId = (string)dictionaryEntry.Value;
                        break;

                }
            }
        }

        public string Domain { get; set; }

        public string Identity { get; set; }

        public string Level { get; set; }

        public string LoggerName { get; set; }

        public string Message { get; set; }

        public DateTime EventTimeStamp { get; set; }

        public string ThreadName { get; set; }

        public string UserName { get; set; }

        public string Location { get; set; }

        public int processId { get; set; }

        public int appDomainId { get; set; }

        public string log4net_HostName { get; set; }

        public string url { get; set; }

        public string sessionId { get; set; }

        ///// <summary>
        ///// Gets or sets a value indicating any custom note associated with this log entry
        ///// </summary>
        //public string CustomNote { get; set; }

        /// <summary>
        /// Cast to a lightweight POCO for serialization
        /// </summary>
        /// <param name="logTableEntity"></param>
        /// <returns></returns>
        public static explicit operator LogItemIntro(LogTableEntity logTableEntity)
        {
            return new LogItemIntro()
                {
                    PartitionKey = logTableEntity.PartitionKey,
                    RowKey = logTableEntity.RowKey,
                    Level = logTableEntity.Level != null ? (Level)Enum.Parse(typeof(Level), logTableEntity.Level) : Models.Level.INFO,
                    LoggerName = logTableEntity.LoggerName,
                    MessageIntro = logTableEntity.Message != null && logTableEntity.Message.Length > 100 ? logTableEntity.Message.Substring(0, 97) + "..." : logTableEntity.Message, // limit message size
                    EventTimestamp = logTableEntity.EventTimeStamp,
                    HostName = logTableEntity.log4net_HostName
                };
        }

        /// <summary>
        /// Cast to a more detailed POCO for serialization
        /// </summary>
        /// <param name="logTableEntity"></param>
        /// <returns></returns>
        public static explicit operator LogItemDetail(LogTableEntity logTableEntity)
        {
            return new LogItemDetail()
                {
                    Timestamp = logTableEntity.Timestamp.LocalDateTime,
                    Domain = logTableEntity.Domain,
                    Identity = logTableEntity.Identity,
                    Message = logTableEntity.Message,
                    ThreadName = logTableEntity.ThreadName,
                    UserName = logTableEntity.UserName,
                    Location = logTableEntity.Location,
                    ProcessId = logTableEntity.processId,
                    AppDomainId = logTableEntity.appDomainId,
                    Url = logTableEntity.url
                };
        }
    }
}
