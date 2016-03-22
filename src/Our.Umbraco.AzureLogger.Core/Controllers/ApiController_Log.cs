namespace Our.Umbraco.AzureLogger.Core.Controllers
{
    using Our.Umbraco.AzureLogger.Core;
    using Our.Umbraco.AzureLogger.Core.Models;
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System.Linq;
    using System.Web.Http;

    public partial class ApiController
    {
        [HttpPost]
        public LogItemIntro[] ReadLogItemIntros([FromUri] string rowKey, [FromUri] int take, [FromBody] SearchItem searchItem)
        {
            return TableService
                    .Instance
                    .ReadLogTableEntities(searchItem.MinLevel,
                                         searchItem.HostName,
                                         searchItem.LoggerNamesInclude,
                                         searchItem.LoggerNames.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray(),
                                         rowKey)
                    .Take(take)
                    .Select(x => (LogItemIntro)x)
                    .ToArray();
        }

        /// <summary>
        /// Get more detailed information on a log item
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
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

        ////[Umbraco.Web.WebApi.UmbracoAuthorize(Roles="")]
        //[HttpGet]
        //public void DeleteLog()
        //{
        //    TableService.Instance.DeleteLog();
        //}
    }
}
