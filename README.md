# Umbraco Azure Logger

Project aims:

1. Implement a Umbraco compatible Log4Net appender which stores the logs into a Azure Storage table. 
This may use https://www.nuget.org/packages/log4net.Appender.Azure or reuse the codebase if not totally suitable.
Need to ensure that several appenders can be used and also that different servers can be identified
2.	Create a Umbraco UI similar to the Diplo Log Viewer for viewing and querying logs from the Azure Table storage, ensuring querying using the Azure API. Need to be able to select appender and server or see merged results. https://our.umbraco.org/projects/developer-tools/diplo-trace-log-viewer/