# Umbraco Azure Logger

This Umbraco package adds a appender for log4net that uses a Azure Storage table instead of the file system to store your Umbraco logs. Also included is a  UI for viewing and querying your log4net entries.

Once installed, edit the ConnectionString parameter the the new appender section of log4net.config in the Config folder to your Azure Storage connection string, you can also change the table name if you want to.

If your site is making a lot of log entries (e.g. you've set to DEBUG level) you should adjust the bufferSize value, this sets how often the logs are written to storage.

Example:

    <root>
    	<priority value="Debug"/>
    	<appender-ref ref="TableAppender" />
    </root>

    <appender name="TableAppender" type="Our.Umbraco.AzureLogger.Core.TableAppender, Our.Umbraco.AzureLogger.Core">
    	<param name="ConnectionString" value="DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=somecrazyrandomtokenthing"/>
    	<param name="TableName" value="UmbracoTraceLog"/>
    	<bufferSize value="1"/>
     </appender>

As a useful enhancement we also now store to URL which triggered the log entry to be made, this can be very handy for tracking down issues.

![Url Example](https://raw.githubusercontent.com/CrumpledDog/Umbraco-Azure-Logger/develop/docs/url-example.png)

Currently this package is available only as a pre-release NuGet package from MyGet https://www.myget.org/gallery/umbracoazurelogger We will create a Umbraco installer package before RTM.