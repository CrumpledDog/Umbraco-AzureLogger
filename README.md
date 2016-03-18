# Umbraco Azure Logger

This Umbraco packages adds a appender for log4net that uses a Azure Storage table instead of the file system to store your Umbraco logs. Also included is a  UI for viewing and querying your log4net entries.

Once installed, edit the ConnectionString parameter the the new appender section of log4net.config in the Config folder to your Azure Storage connection string, you can also change the table name if you want to.

If your site is making a lot of log entries (e.g. you've set to DEBUG level) you should adjust the bufferSize value, this sets how often the logs are written to storage.

Example:

    <appender name="AzureTableAppender" type="log4net.Appender.Umbraco.AzureTableAppender, log4net.Appender.Azure.Umbraco">
    	<param name="TableName" value="UmbracoTraceLog"/>
    	<!-- You can either specify a connection string or use the ConnectionStringName property instead -->
    	<!--<param name="ConnectionString" value="UseDevelopmentStorage=true"/>-->
    	<param name="ConnectionString" value="DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=somecrazyrandomtokenthing"/>
    	<param name="PropAsColumn" value="true"/>
    	<param name="PartitionKeyType" value="DateReverse"/>
    	<bufferSize value="1"/>
    </appender>