namespace Our.Umbraco.AzureLogger.Core.Controllers
{
    using Our.Umbraco.AzureLogger.Core;
    using Our.Umbraco.AzureLogger.Core.Models;
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System.Web.Http;

    /// <summary>
    /// This partial contains CRUD methods for the SearchItem (aka saved search) items
    /// </summary>
    public partial class ApiController
    {
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

        /// <summary>
        /// Saves the new filter state for a search item
        /// </summary>
        /// <param name="searchItemId"></param>
        /// <param name="searchItem"></param>
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
