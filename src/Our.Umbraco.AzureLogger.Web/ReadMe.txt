To configure this in Umbraco:

1) Update /Config/log4net.config to include:

  <root>
    <priority value="Debug"/>
    <appender-ref ref="TableAppender" />
  </root>

  <appender name="TableAppender" type="Our.Umbraco.AzureLogger.Core.TableAppender, Our.Umbraco.AzureLogger.Core">

    <param name="ConnectionString" value="UseDevelopmentStorage=true"/><!-- can either be the name of a connection string in web.config or the full connection string -->
    <param name="TableName" value="LogTable"/><!-- Azure table to insert log entries into -->
	<param name="TreeName" value="friendly text"/><!-- override the name rendered in the Umbraco tree (if not supplied then defaults to the appender name) -->
	<param name="IconName" value="icon-list"/><!-- override the icon rendered in the Umbraco tree (if not supplied then defaults to 'icon-list') -->

 </appender>


