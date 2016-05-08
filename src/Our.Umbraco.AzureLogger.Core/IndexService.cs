namespace Our.Umbraco.AzureLogger.Core
{
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System.Collections.Generic;

    internal sealed partial class IndexService
    {
        private static readonly IndexService indexService = new IndexService();

        static IndexService()
        {
        }

        /// <summary>
        /// prevent external construction (as is singleton)
        /// </summary>
        private IndexService()
        {
        }

        internal static IndexService Instance
        {
            get
            {
                return indexService;
            }
        }

        /// <summary>
        /// Update the machine names and logger names collections
        /// </summary>
        /// <param name="appenderName"></param>
        /// <param name="logTableEntities"></param>
        internal void Process(string appenderName, IEnumerable<LogTableEntity> logTableEntities)
        {

        }

        /// <summary>
        /// Get a distinct collection of all machine names in this log
        /// </summary>
        /// <param name="appenderName"></param>
        /// <returns></returns>
        internal string[] GetMachineNames(string appenderName)
        {
            return new string[] { };
        }

        /// <summary>
        /// Get a distinct collection of all logger names in this log
        /// </summary>
        /// <param name="appenderName"></param>
        /// <returns></returns>
        internal string[] GetLoggerNames(string appenderName)
        {
            return new string[] { };
        }

    }
}
