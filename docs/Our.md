This Umbraco package adds a log4net appender that uses Azure table storage and extends the Umbraco developer tree with functionality to view these logs, (using Azure table storage for logs in preference to the file-system has an additional benefit of reducing file replication activity in load balanced environments where the file system is being synchronised such as Azure Web Apps).

Once installed, edit the ConnectionString named "LoggingTableStorage" added to web.config include the name and key of your Azure storage account (ensure the account has the Table service enabled).

By default the installer will create two appenders and loggers, the first one (AllTableAppender) captures all logs and the second one (WarningsTableAppender) captures only logs of warn level or above. You find you wish to add further appenders and loggers, here is an example for capturing publish events & xml refresh events using 2 loggers and one appender.

      <logger name="Umbraco.Core.Publishing.PublishingStrategy">
    	<appender-ref ref="PublishedTableAppender"/>
      </logger>
    
      <logger name="umbraco.content">
    	<appender-ref ref="PublishedTableAppender"/>
      </logger>
    
      <appender name="PublishedTableAppender" type="Our.Umbraco.AzureLogger.Core.TableAppender, Our.Umbraco.AzureLogger.Core">
	    <param name="ConnectionString" value="LoggingTableStorage"/>
	    <param name="TableName" value="UALPublished"/>
	    <param name="TreeName" value="Published"/>
	    <bufferSize value="0"/>
      </appender>

As a useful enhancement we also now store to URL and SessionId which triggered the log entry to be made, this can be very handy for tracking down issues.

## NuGet ##

NuGet version is available:

    Install-Package Our.Umbraco.AzureLogger -Pre