namespace Our.Umbraco.AzureLogger.Installer
{
    using System;
    using System.Web.Hosting;
    using System.Web.Http;
    using System.Xml;

    using umbraco.cms.businesslogic.packager.standardPackageActions;
    using global::Umbraco.Core;
    using global::Umbraco.Core.Logging;
    using global::Umbraco.Web.Mvc;
    using global::Umbraco.Web.WebApi;

    using Microsoft.WindowsAzure.Storage;

    using Enums;

    [PluginController("AzureLogger")]
    public class InstallerController : UmbracoAuthorizedApiController
    {
        private readonly string _webConfigXdtPath =
            HostingEnvironment.MapPath(string.Format("{0}{1}.install.xdt",
                Constants.InstallerPath,
                Constants.WebConfigFile));

        private readonly string _webConfigPath =
            HostingEnvironment.MapPath(string.Format("~/{0}",
                Installer.Constants.WebConfigFile));

        // /Umbraco/backoffice/AzureLogger/Installer/PostConnectionString
        [HttpPost]
        public InstallerStatus PostConnectionString([FromBody] string connectionString)
        {
            // Check connection string is valid
            if (!TestAzureCredentials(connectionString))
            {
                return InstallerStatus.ConnectionError;
            }

            // Save connectionString to XDT
            if (!SaveConnectionStringoWebConfigXdt(_webConfigXdtPath, connectionString))
            {
                return InstallerStatus.SaveXdtError;
            }

            // Execute Web.Config XDT transform
            if (!ExecuteWebConfigTransform())
            {
                return InstallerStatus.SaveConfigError;
            }

            // Execute Log4Net XDT Transform
            if (!ExecuteLog4NetConfigTransform())
            {
                return InstallerStatus.SaveConfigError;
            }

            return InstallerStatus.Ok;
        }

        // /Umbraco/backoffice/AzureLogger/Installer/GetConnectionString
        [HttpGet]
        public string GetConnectionString()
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=[myAccountName];AccountKey=[myAccountKey]";
            var connectionStringFromConfig = GetConnectionStringFromWebConfig();

            if (connectionStringFromConfig != null)
            {
                connectionString = connectionStringFromConfig;
            }

            return connectionString;
        }

        private static bool SaveConnectionStringoWebConfigXdt(string xdtPath, string connectionString)
        {
            var result = false;

            var document = XmlHelper.OpenAsXmlDocument(xdtPath);

            var nsMgr = new XmlNamespaceManager(document.NameTable);
            var strNamespace = "http://schemas.microsoft.com/XML-Document-Transform";
            nsMgr.AddNamespace("xdt", strNamespace);

            var locationElement = document.SelectSingleNode(string.Format("//configuration/connectionStrings/add[@name=\'{0}\']", Constants.ConnectionName));
            if (locationElement != null && locationElement.Attributes != null)
            {
                locationElement.Attributes["connectionString"].Value = connectionString;
            }

            try
            {
                document.Save(xdtPath);

                // No errors so the result is true
                result = true;
            }
            catch (Exception e)
            {
                // Log error message
                var message = "Error saving XDT Parameters: " + e.Message;
                LogHelper.Error(typeof (InstallerController), message, e);
            }

            return result;
        }

        private static bool ExecuteWebConfigTransform()
        {
            var transFormConfigAction =
                helper.parseStringToXmlNode(string.Format(
                    "<Action runat=\"install\" undo=\"true\" alias=\"{0}\" file=\"~/{1}\" xdtfile=\"{2}{1}\">" +
                    "</Action>",Constants.TransformConfigAlias, Constants.WebConfigFile, Constants.InstallerPath)).FirstChild;

            var transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("AzureLogger", transFormConfigAction);
        }

        private static bool ExecuteLog4NetConfigTransform()
        {
            var transFormConfigAction =
                helper.parseStringToXmlNode(string.Format(
                    "<Action runat=\"install\" undo=\"true\" alias=\"{0}\" file=\"{1}{2}\" xdtfile=\"{3}{2}\">" +
                    "</Action>",Constants.TransformConfigAlias, Constants.UmbracoConfigPath, Constants.Log4NetConfigFile, Constants.InstallerPath)).FirstChild;

            var transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("AzureLogger", transFormConfigAction);
        }

        /// <summary>
        /// Helper method to discover if table storage is supported
        /// </summary>
        /// <param name="connectionString">connection to test</param>
        /// <returns>true if table storage is supported, otherwise false</returns>
        private static bool TestAzureCredentials(string connectionString)
        {
            try
            {
                // Create and delete a test table to ensure storage account supports tables
                var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
                var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
                var testTable = cloudTableClient.GetTableReference("UALTestWritePermission");
                testTable.CreateIfNotExists();
                testTable.DeleteIfExists();
            }
            catch (Exception e)
            {
                LogHelper.Error<InstallerController>(string.Format("Error validating Azure storage connection: {0}", e.Message), e);
                return false;
            }

            return true;
        }

        private string GetConnectionStringFromWebConfig()
        {
            var document = XmlHelper.OpenAsXmlDocument(_webConfigPath);

            var addElement = document.SelectSingleNode(string.Format("//configuration/connectionStrings/add[@name=\'{0}\']", Constants.ConnectionName));

            if (addElement != null && addElement.Attributes != null)
            {
                return addElement.Attributes["connectionString"].Value;
            }

            return null;
        }
    }
}
