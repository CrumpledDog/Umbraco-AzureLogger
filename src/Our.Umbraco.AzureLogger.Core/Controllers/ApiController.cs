namespace Our.Umbraco.AzureLogger.Core.Controllers
{
    using global::Umbraco.Web.Mvc;
    using global::Umbraco.Web.WebApi;
    using Our.Umbraco.AzureLogger.Core;
    using Our.Umbraco.AzureLogger.Core.Models;
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System;
    using System.Linq;
    using System.Web.Http;

    [PluginController("AzureLogger")]
    public class ApiController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Gets all known machine names and logger names (for the auto-suggest)
        /// </summary>
        /// <param name="appenderName"></param>
        /// <returns></returns>
        [HttpGet]
        public object GetIndexes([FromUri]string appenderName)
        {
            return new
            {
                machineNames = IndexService.Instance.GetMachineNames(appenderName),
                loggerNames = IndexService.Instance.GetLoggerNames(appenderName)
            };
        }

        /// <summary>
        /// Gets as many log items as possible (within a single query, or a set duration) to render in the main list
        /// </summary>
        /// <param name="appenderName">name of the log4net appender</param>
        /// <param name="partitionKey">the last known partitionKey (or null)</param>
        /// <param name="rowKey">the last known rowKey (or null)</param>
        /// <param name="queryFilters">filter critera: debug level, machine name, logger name, message text, session id</param>
        /// <returns></returns>
        [HttpPost]
        public object ReadLogItemIntros([FromUri]string appenderName, [FromUri] string partitionKey, [FromUri] string rowKey, [FromBody] dynamic queryFilters)
        {
            // initialise default return values
            LogItemIntro[] logItemIntros = new LogItemIntro[] { };
            string lastPartitionKey = null;
            string lastRowKey = null;
            bool finishedLoading = false;

            logItemIntros = TableService
                            .Instance
                            .ReadLogTableEntities(
                                    appenderName,
                                    partitionKey,
                                    rowKey,
                                    (string)queryFilters.hostName,
                                    (string)queryFilters.loggerName,
                                    (Level)Math.Max((int)queryFilters.minLevel, 0),
                                    (string)queryFilters.message,
                                    (string)queryFilters.sessionId)
                            .Select(x => (LogItemIntro)x)
                            .ToArray();

            if (logItemIntros.Any())
            {
                lastPartitionKey = logItemIntros.Last().PartitionKey;
                lastRowKey = logItemIntros.Last().RowKey;
            }
            else
            {
                finishedLoading = true;
            }

            return new
            {
                logItemIntros = logItemIntros,
                lastPartitionKey = lastPartitionKey,
                lastRowKey = lastRowKey,
                finishedLoading = finishedLoading
            };
        }

        /// <summary>
        /// Get more detailed information on a log item
        /// </summary>
        /// <param name="appenderName">name of the log4net appender</param>
        /// <param name="partitionKey">the partition key for the log item</param>
        /// <param name="rowKey">the row key for the log item</param>
        /// <returns></returns>
        [HttpGet]
        public LogItemDetail ReadLogItemDetail([FromUri]string appenderName, [FromUri]string partitionKey, [FromUri]string rowKey)
        {
            LogTableEntity logTableEntity = TableService.Instance.ReadLogTableEntity(appenderName, partitionKey, rowKey);

            if (logTableEntity != null)
            {
                return (LogItemDetail)logTableEntity;
            }

            return null;
        }

        /// <summary>
        /// wipes all items from the log
        /// </summary>
        /// <param name="appenderName">name of the log4net appender</param>
        [HttpPost]
        public void WipeLog([FromUri]string appenderName)
        {
            TableService.Instance.WipeLog(appenderName);
        }
    }
}
