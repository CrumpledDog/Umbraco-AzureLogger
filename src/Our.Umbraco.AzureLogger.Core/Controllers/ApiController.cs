namespace Our.Umbraco.AzureLogger.Core.Controllers
{
    using global::Umbraco.Web.Mvc;
    using global::Umbraco.Web.WebApi;
    using Our.Umbraco.AzureLogger.Core;
    using Our.Umbraco.AzureLogger.Core.Models;
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System.Linq;
    using System.Web.Http;

    [PluginController("AzureLogger")]
    public class ApiController : UmbracoAuthorizedApiController
    {
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
            try
            {
                // return an array of data found (if size less than the take, then it's the end of the log)
                return TableService
                        .Instance
                        .ReadLogTableEntities(
                                appenderName,
                                partitionKey,
                                rowKey,
                                (string)queryFilters.hostName,
                                (string)queryFilters.loggerName,
                                (string)queryFilters.messageIntro,
                                take) // need to supply take, as result may be a cloud table with items removed (in which case the take doesn't know where to re-start from)
                        .Select(x => (LogItemIntro)x)
                        .ToArray();
            }
            catch (TableQueryTimeoutException exception)
            {
                // return an object detailing where to start the next search (to avoid duplicate processing)
                // may also contain an array of any data found before it timedout
                return new
                {
                    lastPartitionKey = exception.LastPartitionKey,
                    lastRowKey =  exception.LastRowKey,
                    data = exception.Data.Select(x => (LogItemIntro)((LogTableEntity)x)).ToArray()
                };
            }
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
