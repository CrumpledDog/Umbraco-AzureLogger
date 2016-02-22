namespace UmbracoAzureLogger.Core.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;
    using Umbraco.Web.Mvc;
    using Umbraco.Web.WebApi;
    using UmbracoAzureLogger.Core.Models;

    [PluginController("AzureLogger")]
    public class ApiController : UmbracoAuthorizedApiController
    {
        [HttpGet]
        public object Connect()
        {
            string connectionErrorMessage = null;

            try
            {
                LogTableService.Instance.Connect();
            }
            catch(Exception exception)
            {
                connectionErrorMessage = exception.Message;
            }

            return new
            {
                connected = LogTableService.Instance.Connected.Value, // will have a value, as set in the Connect() method
                connectionErrorMessage = connectionErrorMessage,
                connectionString = LogTableService.Instance.ConnectionString,
                tableName = LogTableService.Instance.TableName
            };
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
            return LogTableService
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
            LogTableEntity logTableEntity = LogTableService.Instance.GetLogTableEntity(partitionKey, rowKey);

            if (logTableEntity != null)
            {
                return (LogItemDetail)logTableEntity;
            }

            return null;
        }

        //[Umbraco.Web.WebApi.UmbracoAuthorize(Roles="")]
        [HttpGet]
        public void DeleteLog()
        {
            LogTableService.Instance.DeleteLog();
        }
    }
}
