namespace Our.Umbraco.AzureLogger.Core.Models.TableEntities
{
    using Microsoft.WindowsAzure.Storage.Table;
    using System;

    /// <summary>
    /// This the data for a 'saved search' in the tree (potentially could also user / group indexed)
    /// </summary>
    public class SearchItemTableEntity : TableEntity
    {
        public string Name { get; set; }

        public string MinLevel { get; set; }

        public string HostName { get; set; }

        //public string[] HostNames { get; set; }

        //public bool HostNamesInclude { get; set; } // false means exclude

        public bool LoggerNamesInclude { get; set;  } // false means exclude

        // pipe | delimited list of logger names (simple serialization as Azure table storage doens't support arrays)
        public string LoggerNames { get; set; }

        /// <summary>
        /// Cast to a poco for json serialization (without the weight of inherited TableEntity properties)
        /// </summary>
        /// <param name="searchItemTableEntity"></param>
        /// <returns></returns>
        public static explicit operator SearchItem(SearchItemTableEntity searchItemTableEntity)
        {
            return new SearchItem()
                {
                    Name = searchItemTableEntity.Name,
                    MinLevel = searchItemTableEntity.MinLevel != null ? (Level)Enum.Parse(typeof(Level), searchItemTableEntity.MinLevel) : Level.DEBUG,
                    HostName = searchItemTableEntity.HostName,
                    LoggerNamesInclude = searchItemTableEntity.LoggerNamesInclude,
                    LoggerNames = searchItemTableEntity.LoggerNames.Split('|')
                };
        }
    }
}
