namespace Our.Umbraco.AzureLogger.Core.Models.TableEntities
{
    using Microsoft.WindowsAzure.Storage.Table;
    using System;

    /// <summary>
    /// POCO to match the field names in the Azure table (NOTE: casing of properties matches the Azure field names)
    /// </summary>
    public class LogTableEntity : TableEntity
    {
        public string Domain { get; set; }

        public string Identity { get; set; }

        public string Level { get; set; }

        public string LoggerName { get; set; }

        public string Message { get; set; }

        public DateTime EventTimeStamp { get; set; }

        public string ThreadName { get; set; } // int didn't work

        public string UserName { get; set; }

        public string Location { get; set; }

        public int processId { get; set; }

        public int appDomainId { get; set; }

        public string log4net_HostName { get; set; }

        public string url { get; set; }

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
