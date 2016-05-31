using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml;
using Microsoft.WindowsAzure.Storage;
using Our.Umbraco.AzureLogger.Installer.Enums;
using umbraco.cms.businesslogic.packager.standardPackageActions;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Our.Umbraco.AzureLogger.Installer
{
    [PluginController("AzureLogger")]
    public class InstallerController : UmbracoAuthorizedApiController
    {
        private readonly string log4netConfigInstallXdtPath = HostingEnvironment.MapPath(string.Format("{0}{1}.install.xdt", global::Our.Umbraco.AzureLogger.Installer.Constants.InstallerPath,global::Our.Umbraco.AzureLogger.Installer.Constants.Log4NetConfigFile));
        private readonly string log4netConfigPath = HostingEnvironment.MapPath(string.Format("{0}{1}", global::Our.Umbraco.AzureLogger.Installer.Constants.UmbracoConfigPath,global::Our.Umbraco.AzureLogger.Installer.Constants.Log4NetConfigFile));

        private readonly string webConfigXdtPath = HostingEnvironment.MapPath(string.Format("{0}{1}.install.xdt", global::Our.Umbraco.AzureLogger.Installer.Constants.InstallerPath,global::Our.Umbraco.AzureLogger.Installer.Constants.WebConfigFile));
        private readonly string webConfigPath = HostingEnvironment.MapPath(string.Format("~/{0}", global::Our.Umbraco.AzureLogger.Installer.Constants.WebConfigFile));

        // /Umbraco/backoffice/AzureLogger/Installer/PostParameters
        [HttpPost]
        public InstallerStatus PostParameters([FromBody]string connectionString)
        {
            // Check connection string is valid
            if (!TestAzureCredentials(connectionString))
            {
                return InstallerStatus.ConnectionError;
            }

            // Save connectionString to XDT
            if (!SaveConnectionStringoWebConfigXdt(webConfigXdtPath, connectionString))
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

        private static bool SaveConnectionStringoWebConfigXdt(string xdtPath, string connectionString)
        {
            var result = false;

            var document = XmlHelper.OpenAsXmlDocument(xdtPath);

            // Inset a Parameter element with Xdt remove so that updated values get saved (for upgrades), we don't want this for NuGet packages which is why it's here instead
            var nsMgr = new XmlNamespaceManager(document.NameTable);
            var strNamespace = "http://schemas.microsoft.com/XML-Document-Transform";
            nsMgr.AddNamespace("xdt", strNamespace);

            var locationElement = document.SelectSingleNode("//configuration/connectionStrings/add[@name=\'LoggingTableStorage\']");
            if (locationElement != null)
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
                LogHelper.Error(typeof(InstallerController), message, e);
            }

            return result;
        }

        private static bool ExecuteWebConfigTransform()
        {
            var transFormConfigAction = helper.parseStringToXmlNode("<Action runat=\"install\" undo=\"true\" alias=\"AzureLogger.Azure.TransformConfig\" file=\"~/web.config\" xdtfile=\"~/app_plugins/AzureLogger/install/web.config\">" +
         "</Action>").FirstChild;

            var transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("AzureLogger", transFormConfigAction);
        }

        private static bool ExecuteLog4NetConfigTransform()
        {
            var transFormConfigAction = helper.parseStringToXmlNode("<Action runat=\"install\" undo=\"true\" alias=\"AzureLogger.Azure.TransformConfig\" file=\"~/config/log4net.config\" xdtfile=\"~/app_plugins/AzureLogger/install/log4net.config\">" +
         "</Action>").FirstChild;

            var transformConfig = new PackageActions.TransformConfig();
            return transformConfig.Execute("AzureLogger", transFormConfigAction);
        }

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
    }
}
