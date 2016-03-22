namespace Our.Umbraco.AzureLogger.Core.Controllers
{
    using global::Umbraco.Web.Mvc;
    using global::Umbraco.Web.WebApi;
    using Our.Umbraco.AzureLogger.Core;
    using System;
    using System.Web.Http;

    [PluginController("AzureLogger")]
    public partial class ApiController : UmbracoAuthorizedApiController
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
    }
}
