namespace UmbracoAzureLogger.Core.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;
    using Umbraco.Web.Mvc;
    using Umbraco.Web.WebApi;
    using UmbracoAzureLogger.Core.Models;
    using UmbracoAzureLogger.Core.Models.TableEntities;

    [PluginController("AzureLogger")]
    public class ApiController : UmbracoAuthorizedApiController
    {
        [HttpGet]
        public object Connect()
        {
            string connectionErrorMessage = null;

            try
            {
                TableService.Instance.Connect();
            }
            catch(Exception exception)
            {
                connectionErrorMessage = exception.Message;
            }

            return new
            {
                connected = TableService.Instance.Connected.Value, // will have a value, as set in the Connect() method
                connectionErrorMessage = connectionErrorMessage,
                connectionString = TableService.Instance.ConnectionString,
                tableName = TableService.Instance.TableName
            };
        }

        /// <summary>
        /// Creates a new SearchItem (a saved search using supplied name)
        /// </summary>
        /// <param name="name"></param>
        [HttpPost]
        public void CreateSearchItem([FromUri] string name)
        {
            TableService.Instance.InsertSearchItemTableEntity(name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchItemId">also used as the Azure table rowKey</param>
        /// <returns></returns>
        [HttpGet]
        public SearchItem ReadSearchItem([FromUri] string searchItemId)
        {
            SearchItemTableEntity searchItemTableEntity = TableService.Instance.GetSearchItemTableEntity(searchItemId);

            if (searchItemTableEntity != null)
            {
                return (SearchItem)searchItemTableEntity;
            }

            return null;
        }

        [HttpPost]
        public void UpdateSearchItem(
                            [FromUri] string searchItemId,
                            [FromUri] Level minLevel,
                            [FromUri] string hostName,
                            [FromUri] string loggerName)
        {
            //TableService.Instance.UpdateSearchItemTableEntity(new SearchItemTableEntity() {  })


        }

        /// <summary>
        /// Deletes a SearchItem (aka saved search) from the Azure table
        /// </summary>
        /// <param name="rowKey">the guid id</param>
        [HttpPost]
        public void DeleteSearchItem([FromUri] string searchItemId)
        {
            TableService.Instance.DeleteSearchItemTableEntity(searchItemId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rowKey">(optional) the row key to start from</param>
        /// <param name="take">max number of items to return</param>
        /// <returns>a collection of <see cref="LogItemIntro"/> objects</returns>
        [HttpGet]
        public LogItemIntro[] GetLogItemIntros(
                                [FromUri]Level minLevel,
                                [FromUri]string hostName,
                                [FromUri]string loggerName,
                                [FromUri]string rowKey,
                                [FromUri]int take) // TODO: add partition key ?
        {
            return TableService
                    .Instance
                    .GetLogTableEntities(minLevel, hostName, loggerName, rowKey)
                    .Take(take)
                    .Select(x => (LogItemIntro)x).ToArray();
        }

        /// <summary>
        /// Get more detailed information on a log item
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        [HttpGet]
        public LogItemDetail GetLogItemDetail([FromUri]string partitionKey, [FromUri]string rowKey)
        {
            LogTableEntity logTableEntity = TableService.Instance.GetLogTableEntity(partitionKey, rowKey);

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
