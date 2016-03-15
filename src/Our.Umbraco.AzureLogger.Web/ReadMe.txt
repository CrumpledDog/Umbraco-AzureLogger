To configure this in Umbraco:

1) Update /Config/log4net.config to include:

  <root>
    <priority value="Debug"/>
    <!--<appender-ref ref="AsynchronousLog4NetAppender" />-->
    <appender-ref ref="AzureTableAppender" />
  </root>

  <appender name="AzureTableAppender" type="log4net.Appender.AzureTableAppender, log4net.Appender.Azure">
    <param name="TableName" value="LogTable"/>

    <!-- You can either specify a connection string or use the ConnectionStringName property instead -->
    <param name="ConnectionString" value="UseDevelopmentStorage=true"/>
    <!--<param name="ConnectionStringName" value="GlobalConfigurationString" />-->

    <!-- You can specify this to make each LogProperty as separate Column in TableStorage,
    Default: all Custom Properties were logged into one single field -->
    <param name="PropAsColumn" value="true" />

   <param name="PartitionKeyType" value="DateReverse" /> <!-- LoggerName-->
 </appender>


