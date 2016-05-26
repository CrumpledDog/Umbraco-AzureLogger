# Umbraco Azure Logger

![Azure Logger](build/assets/icon/umbraco-azure-logger-256.png)

[![Build status](https://ci.appveyor.com/api/projects/status/ivwi8cxt3cs05xxe?svg=true)](https://ci.appveyor.com/project/JeavonLeopold/umbraco-azure-logger)

This Umbraco package adds a appender for log4net that uses a Azure Storage table instead of the file system to store your Umbraco logs. Also included is a  UI for viewing and querying your log4net entries.

Once installed, edit the ConnectionString named "LoggingTableStorage" added to web.config include the name and key of your Azure storage account (ensure the account has the Table service enabled).

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

As a useful enhancement we also now store to URL which triggered the log entry to be made, this can be very handy for tracking down issues.

![Url Example](https://raw.githubusercontent.com/CrumpledDog/Umbraco-Azure-Logger/develop/docs/url-example.png)

## Installation ##

Currently only NuGet packages are available

|NuGet Packages    |Version           |
|:-----------------|:-----------------|
|**Release**|[![NuGet download](http://img.shields.io/nuget/v/Our.Umbraco.AzureLogger.svg)](https://www.nuget.org/packages/Our.Umbraco.AzureLogger/)
|**Pre-release**|[![MyGet download](https://img.shields.io/myget/umbraco-packages/vpre/Our.Umbraco.AzureLogger.svg)](https://www.myget.org/gallery/umbraco-packages)

