namespace Our.Umbraco.AzureLogger.Core.Controllers
{
    using Our.Umbraco.AzureLogger.Core;
    using Our.Umbraco.AzureLogger.Core.Models;
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System.Web.Http;

    public partial class ApiController
    {
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
        public void UpdateSearchItem([FromUri] string searchItemId, [FromBody] SearchItem searchItem)
        {
            TableService
                .Instance
                .UpdateSearchItemTableEntity(
                    searchItemId,
                    searchItem.MinLevel,
                    searchItem.HostName,
                    searchItem.LoggerNamesInclude,
                    searchItem.LoggerNames);
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
    }
}
