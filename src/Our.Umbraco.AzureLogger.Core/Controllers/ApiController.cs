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
        /// Gets the log items to render in the main list
        /// </summary>
        /// <param name="appenderName">name of the log4net appender</param>
        /// <param name="partitionKey">the last known partitionKey</param>
        /// <param name="rowKey">the last known rowKey</param>
        /// <param name="take">number of log items to get</param>
        /// <param name="queryFilters">using dynamic to avoid making a strongly typed model</param>
        /// <returns></returns>
        [HttpPost]
        public object ReadLogItemIntros([FromUri]string appenderName, [FromUri] string partitionKey, [FromUri] string rowKey, [FromUri] int take, [FromBody] dynamic queryFilters)
        {
            LogItemIntro[] logItemIntros;
            string lastPartitionKey = null;
            string lastRowKey= null;
            bool finishedLoading = false;

            try
            {
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
                                (string)queryFilters.sessionId,
                                take) // need to supply take, as result may be a cloud table with items removed (in which case the take doesn't know where to re-start from)
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
            }
            catch (TableQueryTimeoutException exception)
            {
                logItemIntros = exception.Data.Select(x => (LogItemIntro)((LogTableEntity)x)).ToArray();
                lastPartitionKey = exception.LastPartitionKey;
                lastRowKey =  exception.LastRowKey;
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
