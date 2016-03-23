namespace Our.Umbraco.AzureLogger.Core.Controllers
{
    using Our.Umbraco.AzureLogger.Core;
    using Our.Umbraco.AzureLogger.Core.Models;
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System.Linq;
    using System.Web.Http;

    /// <summary>
    /// This partial contains read methods for Log items
    /// </summary>
    public partial class ApiController
    {
        /// <summary>
        /// Gets the log items to render in the main list
        /// </summary>
        /// <param name="partitionKey">the last known partitionKey</param>
        /// <param name="rowKey">the last known rowKey</param>
        /// <param name="take">number of log items to get</param>
        /// <param name="searchItem">object representing the state of all the filters</param>
        /// <returns></returns>
        [HttpPost]
        public LogItemIntro[] ReadLogItemIntros([FromUri] string partitionKey, [FromUri] string rowKey, [FromUri] int take, [FromBody] SearchItem searchItem)
        {
            return TableService
                    .Instance
                    .ReadLogTableEntities(
                            partitionKey,
                            rowKey,
                            searchItem.MinLevel,
                            searchItem.HostName,
                            searchItem.LoggerNamesInclude,
                            searchItem.LoggerNames.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray())
                    .Take(take)
                    .Select(x => (LogItemIntro)x)
                    .ToArray();
        }

        /// <summary>
        /// Get more detailed information on a log item
        /// </summary>
        /// <param name="partitionKey">the partition key for the log item</param>
        /// <param name="rowKey">the row key for the log item</param>
        /// <returns></returns>
        [HttpGet]
        public LogItemDetail ReadLogItemDetail([FromUri]string partitionKey, [FromUri]string rowKey)
        {
            LogTableEntity logTableEntity = TableService.Instance.ReadLogTableEntity(partitionKey, rowKey);

            if (logTableEntity != null)
            {
                return (LogItemDetail)logTableEntity;
            }

            return null;
        }
    }
}
