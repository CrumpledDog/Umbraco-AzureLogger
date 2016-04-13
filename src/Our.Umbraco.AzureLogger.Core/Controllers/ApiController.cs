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
        /// <param name="appenderName"></param>
        /// <param name="partitionKey">the last known partitionKey</param>
        /// <param name="rowKey">the last known rowKey</param>
        /// <param name="take">number of log items to get</param>
        /// <returns></returns>
        [HttpGet]
        public LogItemIntro[] ReadLogItemIntros([FromUri]string appenderName, [FromUri] string partitionKey, [FromUri] string rowKey, [FromUri] int take)
        {
            return TableService
                    .Instance
                    .ReadLogTableEntities(
                            appenderName,
                            partitionKey,
                            rowKey)
                    .Take(take)
                    .Select(x => (LogItemIntro)x)
                    .ToArray();
        }

        /// <summary>
        /// Get more detailed information on a log item
        /// </summary>
        /// <param name="appenderName"></param>
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
    }
}
