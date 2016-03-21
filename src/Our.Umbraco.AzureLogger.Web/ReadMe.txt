To configure this in Umbraco:

1) Update /Config/log4net.config to include:

  <root>
    <priority value="Debug"/>
    <appender-ref ref="TableAppender" />
  </root>

  <appender name="TableAppender" type="Our.Umbraco.AzureLogger.Core.TableAppender, Our.Umbraco.AzureLogger.Core">

    <param name="ConnectionString" value="UseDevelopmentStorage=true"/>
    <param name="TableName" value="LogTable"/>

 </appender>


