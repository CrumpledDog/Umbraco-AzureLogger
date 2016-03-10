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

        public string MinLevel { get; set; }

        public string HostName { get; set; }

        public string LoggerName { get; set; }

        public static explicit operator SearchItem(SearchItemTableEntity searchItemTableEntity)
        {
            return new SearchItem()
                {
                    Name = searchItemTableEntity.Name,
                    MinLevel = searchItemTableEntity.MinLevel != null ? (Level)Enum.Parse(typeof(Level), searchItemTableEntity.MinLevel) : Level.DEBUG,
                    HostName = searchItemTableEntity.HostName,
                    LoggerName = searchItemTableEntity.LoggerName
                };
        }
    }
}
