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
            if (!TestAzureCredentials(connectionString))
            {
                return InstallerStatus.ConnectionError;
            }


            return InstallerStatus.SaveXdtError;
        }


        private static bool TestAzureCredentials(string connectionString)
        {

            try
            {
                var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);

                var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
               // var blobContainer = cloudBlobClient.GetContainerReference(containerName);
                var tableList = cloudTableClient.ListTables();
                // this should fully check that the connection works, the result is not relevant

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
