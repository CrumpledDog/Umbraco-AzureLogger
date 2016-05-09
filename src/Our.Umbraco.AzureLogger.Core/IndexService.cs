﻿namespace Our.Umbraco.AzureLogger.Core
{
    using Our.Umbraco.AzureLogger.Core.Models.TableEntities;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed partial class IndexService
    {
        private static readonly IndexService indexService = new IndexService();

        private ConcurrentDictionary<string, List<string>> appenderMachineNames = new ConcurrentDictionary<string, List<string>>();

        private ConcurrentDictionary<string, List<string>> appenderLoggerNames = new ConcurrentDictionary<string, List<string>>();

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
            IEnumerable<string> machineNames = logTableEntities
                                                .Select(x => x.log4net_HostName)
                                                .Where(x => !this.GetMachineNames(appenderName).Any(y => y == x))
                                                .Distinct();

            if (machineNames.Any())
            {
                // TODO: locking

                TableService.Instance.CreateIndexTableEntities(appenderName, "machineNames", machineNames.ToArray());

                // update local collection
                this.appenderMachineNames[appenderName].AddRange(machineNames);

                // TODO: release locking
            }

            IEnumerable<string> loggerNames = logTableEntities
                                                .Select(x => x.LoggerName)
                                                .Where(x => !this.GetLoggerNames(appenderName).Any(y => y == x))
                                                .Distinct();

            if (loggerNames.Any())
            {
                // TODO: locking

                TableService.Instance.CreateIndexTableEntities(appenderName, "loggerNames", loggerNames.ToArray());

                // update local collection
                this.appenderLoggerNames[appenderName].AddRange(loggerNames);

                // TODO: release locking
            }
        }

        /// <summary>
        /// Get a distinct collection of all machine names in this log
        /// </summary>
        /// <param name="appenderName"></param>
        /// <returns></returns>
        internal List<string> GetMachineNames(string appenderName)
        {
            if (!this.appenderMachineNames.ContainsKey(appenderName))
            {
                this.appenderMachineNames[appenderName] = TableService.Instance.ReadIndexTableEntities(appenderName, "machineNames")
                                                                        .Select(x => x.RowKey)
                                                                        .ToList();
            }

            return this.appenderMachineNames[appenderName];
        }

        /// <summary>
        /// Get a distinct collection of all logger names in this log
        /// </summary>
        /// <param name="appenderName"></param>
        /// <returns></returns>
        internal List<string> GetLoggerNames(string appenderName)
        {
            if (!this.appenderLoggerNames.ContainsKey(appenderName))
            {
                this.appenderLoggerNames[appenderName] = TableService.Instance.ReadIndexTableEntities(appenderName, "loggerNames")
                                                                        .Select(x => x.RowKey)
                                                                        .ToList();
            }

            return this.appenderLoggerNames[appenderName];
        }
    }
}
