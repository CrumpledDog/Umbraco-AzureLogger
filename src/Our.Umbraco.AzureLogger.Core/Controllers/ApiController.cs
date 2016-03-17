namespace Our.Umbraco.AzureLogger.Core.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;

    using global::Umbraco.Web.Mvc;
    using global::Umbraco.Web.WebApi;

    using Core;
    using Models;
    using Models.TableEntities;

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

        #region SearchItem

        /// <summary>
        /// Creates a new SearchItem (a saved search using supplied name)
        /// </summary>
        /// <param name="name"></param>
        [HttpPost]
        public void CreateSearchItem([FromUri] string name)
        {
            TableService.Instance.CreateSearchItemTableEntity(name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="searchItemId">also used as the Azure table rowKey</param>
        /// <returns></returns>
        [HttpGet]
        public SearchItem ReadSearchItem([FromUri] string searchItemId)
        {
            SearchItemTableEntity searchItemTableEntity = TableService.Instance.ReadSearchItemTableEntity(searchItemId);

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
                            [FromUri] bool loggerNamesInclude,
                            [FromUri] string[] loggerNames)
        {
            TableService.Instance.UpdateSearchItemTableEntity(searchItemId, minLevel, hostName, loggerNamesInclude, loggerNames);
        }

        /// <summary>
        /// Deletes a SearchItem (aka saved search) from the Azure table
        /// </summary>
        /// <param name="rowKey">the guid id</param>
        [HttpPost]
        public void DeleteSearchItem([FromUri] string searchItemId)
        {
            // the searchItemId is the same as the Azure table rowKey
            TableService.Instance.DeleteSearchItemTableEntity(searchItemId);
        }

        #endregion

        #region LogItem

        [HttpPost]
        public LogItemIntro[] ReadLogItemIntros([FromUri] string rowKey, [FromUri] int take, [FromBody] SearchItem searchItem)
        {
            return TableService
                    .Instance
                    .ReadLogTableEntities(searchItem.MinLevel,
                                         searchItem.HostName,
                                         searchItem.LoggerNamesInclude,
                                         searchItem.LoggerNames,
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

        #endregion

        ////[Umbraco.Web.WebApi.UmbracoAuthorize(Roles="")]
        //[HttpGet]
        //public void DeleteLog()
        //{
        //    TableService.Instance.DeleteLog();
        //}
    }
}
