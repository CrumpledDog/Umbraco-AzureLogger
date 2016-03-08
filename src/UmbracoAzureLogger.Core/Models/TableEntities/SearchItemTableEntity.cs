namespace UmbracoAzureLogger.Core.Models.TableEntities
{
    using Microsoft.WindowsAzure.Storage.Table;
    using System;

    /// <summary>
    /// This the data for a 'saved search' in the tree (potentially could also user / group indexed)
    /// </summary>
    public class SearchItemTableEntity : TableEntity
    {
        /// <summary>
        /// The name of this 'saved search' item
        /// </summary>
        public string Name { get; set; }

        ///// <summary>
        ///// The state of the filters on this 'saved search' item
        ///// </summary>
        //public SearchFiltersState SearchFiltersState { get; set; }
    }
}
