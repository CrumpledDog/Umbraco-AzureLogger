namespace Our.Umbraco.AzureLogger.Core
{
    using log4net;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed partial class TableService
    {
        private static readonly TableService tableService = new TableService();

        private const string SearchItemPartitionKey = "searchItem";

        static TableService()
        {
        }

        /// <summary>
        /// singleton constructor
        /// </summary>
        private TableService()
        {
        }

        internal static TableService Instance
        {
            get
            {
                return tableService;
            }
        }

        /// <summary>
        ///  null = not yet attempted to connect
        ///  true = connected
        ///  false = connection failed
        /// </summary>
        internal bool? Connected { get; private set;}

        internal string ConnectionString { get; private set; }

        internal string TableName { get; private set; }

        private CloudTable CloudTable { get; set;}

        internal void Connect()
        {
            if (!this.Connected.HasValue) // if connection not yet attempted
            {
                //this.Connected = false; // default

                IEnumerable<TableAppender> tableAppenders = LogManager.GetLogger(typeof(TableAppender)).Logger.Repository.GetAppenders().Cast<TableAppender>();

                //if (!azureTableAppenders.Any())
                //{
                //    throw new Exception("Couldn't find a log4net AzureTableAppender");
                //}
                //else if (azureTableAppenders.Count() > 1)
                //{
                //    throw new Exception("Found more than one log4net AzureTableAppender");
                //}
                //else
                if (tableAppenders.Count() == 1)
                {
                    // we have found a single AzureTableAppender
                    TableAppender tableAppender = tableAppenders.Single();

                    this.ConnectionString = tableAppender.ConnectionString;
                    this.TableName = tableAppender.TableName;

                    CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(this.ConnectionString);

                    CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

                    this.CloudTable = cloudTableClient.GetTableReference(this.TableName);

					// create the storage table if it isn't already there
					this.CloudTable.CreateIfNotExists();

					this.Connected = true;
                }
            }
        }
    }
}
