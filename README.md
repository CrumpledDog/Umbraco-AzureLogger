# Umbraco Azure Logger

![Azure Logger](build/assets/icon/umbraco-azure-logger-256.png)

[![Build status](https://ci.appveyor.com/api/projects/status/ivwi8cxt3cs05xxe?svg=true)](https://ci.appveyor.com/project/JeavonLeopold/umbraco-azure-logger)

This Umbraco package adds a appender for log4net that uses a Azure Storage table instead of the file system to store your Umbraco logs. Also included is a  UI for viewing and querying your log4net entries, this is a tree of Appenders within the developer section of Umbraco.

Once installed, edit the ConnectionString named "LoggingTableStorage" added to web.config include the name and key of your Azure storage account (ensure the account has the Table service enabled).

![Tree Example](docs/tree.png)

If your site is making a lot of log entries (e.g. you've set to DEBUG level) you should adjust the bufferSize value, this sets how often the logs are written to storage.

Example:

	  <root>
	    <priority value="Info"/>
	    <appender-ref ref="AsynchronousLog4NetAppender" />
	    <appender-ref ref="AllTableAppender"/>
	    <appender-ref ref="WarningsTableAppender"/>
	  </root>

	  <appender name="AllTableAppender" type="Our.Umbraco.AzureLogger.Core.TableAppender, Our.Umbraco.AzureLogger.Core">
	    <param name="ConnectionString" value="LoggingTableStorage"/>
	    <param name="TableName" value="UALUmbracoTraceLog"/>
	    <param name="TreeName" value="All Events"/>
	    <bufferSize value="5"/>
	    <!-- 0 indexed -->
	  </appender>

	  <appender name="WarningsTableAppender" type="Our.Umbraco.AzureLogger.Core.TableAppender, Our.Umbraco.AzureLogger.Core">
	    <param name="ConnectionString" value="LoggingTableStorage"/>
	    <param name="TableName" value="UALWarnings"/>
	    <param name="TreeName" value="Warnings"/>
	    <param name="IconName" value="icon-alert"/>
	    <threshold value="WARN"/>
	    <filter type="log4net.Filter.LevelRangeFilter">
	      <levelMin value="WARN"/>
	      <levelMax value="ERROR"/>
	    </filter>
	    <bufferSize value="0"/>
	    <!-- 1 item in buffer -->
	  </appender>

As a useful enhancement we also now store to URL and SessionId which triggered the log entry to be made, this can be very handy for tracking down issues.

![Url Example](https://raw.githubusercontent.com/CrumpledDog/Umbraco-Azure-Logger/develop/docs/url-example.png)

## Installation ##

NuGet & Umbraco packages are available

|NuGet Packages    |Version           |
|:-----------------|:-----------------|
|**Release**|[![NuGet download](http://img.shields.io/nuget/v/Our.Umbraco.AzureLogger.svg)](https://www.nuget.org/packages/Our.Umbraco.AzureLogger/)
|**Pre-release**|[![MyGet download](https://img.shields.io/myget/umbraco-packages/vpre/Our.Umbraco.AzureLogger.svg)](https://www.myget.org/gallery/umbraco-packages)

|Umbraco Packages  |                  |
|:-----------------|:-----------------|
|**Release**|[![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-orange.svg)](https://our.umbraco.org/projects/collaboration/azurelogger/) 
|**Pre-release**| [![AppVeyor Artifacts](https://img.shields.io/badge/appveyor-umbraco-orange.svg)](https://ci.appveyor.com/project/JeavonLeopold/umbraco-azure-logger/build/artifacts)

## Licensing ##

This project is licensed under the [Apache 2.0 license](http://www.apache.org/licenses/LICENSE-2.0).

The project includes [WindowsAzure.Storage](https://www.nuget.org/packages/WindowsAzure.Storage/) licensed under the [Apache License](https://raw.githubusercontent.com/WindowsAzure/azure-sdk-for-net/master/LICENSE.txt) and [Microsoft.Web.Xdt](https://www.nuget.org/packages/Microsoft.Web.Xdt) licensed under [MS-EULA license](https://www.microsoft.com/web/webpi/eula/microsoft_web_xmltransform.htm).
