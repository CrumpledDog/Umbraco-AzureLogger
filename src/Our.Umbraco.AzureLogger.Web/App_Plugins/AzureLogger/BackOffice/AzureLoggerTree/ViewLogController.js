angular
    .module('umbraco')
    .controller('AzureLogger.ViewLogController', [
        '$scope', '$http', '$routeParams', 'navigationService', '$q', '$timeout', 'AzureLogger.AzureLoggerResource',
        function ($scope, $http, $routeParams, navigationService, $q, $timeout, azureLoggerResource) {

            var appenderName = $routeParams.id.split('|')[0];
            $scope.name = $routeParams.id.split('|')[1]; // the appender name, or tree name
            $scope.logItems = [];
            $scope.currentlyLoading = false; // true when getting data awaiting a response to set
            $scope.finishedLoading = false; // true once server indicates that there is no more data
            // TODO: $scope.startEventTimestamp; // set with date picker
            // TODO: $scope.threadIdentity; // built from AppDomainId + ProcessId + ThreadName (set by clicking in details view)
            // TODO: $scope.logItemLimit = 1000; // size of logItems array before it is reset (and a new start date time in the filter)

            var queryFilters = { hostName: '', loggerName: '', minLevel: 0, message: '' }; // the filter state to use for the ajax queries
            $scope.uiFilters = angular.copy(queryFilters); // set the ui filter state to match
            $scope.currentlyFiltering = false;

            // forces the tree to highlight appender used for this view
            // https://our.umbraco.org/forum/umbraco-7/developing-umbraco-7-packages/48870-Make-selected-node-in-custom-tree-appear-selected
            navigationService.syncTree({ tree: 'azureLoggerTree', path: ['-1', 'appender|' + appenderName], forceReload: false });

            // tell the resource that this is now the currently active view
            azureLoggerResource.activeAppenderViewLog = appenderName;


            //$scope.isActiveAppender = function () {
            //    return appenderName == azureLoggerResource.activeAppenderViewLog;
            //};

            // clear all log items currently being viewed
            var clearLogItems = function () {
                $scope.logItems = [];
                $scope.finishedLoading = false;
            };

            // handles any filter ui changes
            $scope.handleFilters = function () {

                $scope.currentlyFiltering = true;

                $timeout(function () { // timeout ensures scope is ready

                    var uiFilterHostName = $scope.uiFilters.hostName.toLowerCase();
                    var uiFilterLoggerName = $scope.uiFilters.loggerName.toLowerCase();
                    var uiFilterMinLevel = $scope.uiFilters.minLevel;
                    var uiFilterMessage = $scope.uiFilters.message.toLowerCase(); // any change in message will be reductive - as not all data client side

                    var queryFilterHostName = queryFilters.hostName.toLowerCase();
                    var queryFilterLoggerName = queryFilters.loggerName.toLowerCase();
                    var queryFilterMinLevel = queryFilters.minLevel;
                    var queryFilterMessage = queryFilters.message.toLowerCase();

                    queryFilters = angular.copy($scope.uiFilters); // update queryFilters as early as possible

                    // when true, indicates that the current result set will be reduced
                    var reductive = uiFilterHostName.indexOf(queryFilterHostName) > -1
                                    && uiFilterLoggerName.indexOf(queryFilterLoggerName) > -1
                                    && uiFilterMinLevel >= queryFilterMinLevel
                                    && uiFilterMessage == queryFilterMessage;

                    // if reductive, then remove all items that don't match (a new query may happen)
                    if (reductive) {

                        // machine name changed
                        if (uiFilterHostName != queryFilterHostName) {
                            $scope.logItems = $scope.logItems.filter(function (value) {
                                return value.hostName.toLowerCase().indexOf(uiFilterHostName) > -1;
                            });
                        }

                        // logger name changed
                        if (uiFilterLoggerName != queryFilterLoggerName) {
                            $scope.logItems = $scope.logItems.filter(function (value) {
                                return value.loggerName.toLowerCase().indexOf(uiFilterLoggerName) > -1;
                            });
                        }

                        // level changed
                        if (uiFilterMinLevel != queryFilterMinLevel)
                        {
                            $scope.logItems = $scope.logItems.filter(function (value) {
                                return value.levelValue >= uiFilterMinLevel;
                            });
                        }

                        // if message changed, then it can't be reductive, as not enough data locally

                    } else {
                        clearLogItems(); // delete all items as we may be missing data (this will trigger a refresh)
                    }

                }) // no delay in timeout
                .then(function () {
                    $scope.currentlyFiltering = false;
                });

            };

            // listen for any 'WipedLog' broadcasts
            $scope.$on('WipedLog', function (event, arg) {
                if (arg == appenderName) { // safety check: if destined for this appender
                    clearLogItems();
                }
            });

            $scope.updateLogItems = function () { // TODO: update head of existing data
                clearLogItems(); // HACK: reloadLogItems so it returns the latest
            };

            var lastPartitionKey = null;
            var lastRowKey = null;

            // returns a promise with a bool result
            $scope.getMoreLogItems = function () {

                var deferred = $q.defer();

                if (appenderName != azureLoggerResource.activeAppenderViewLog || $scope.currentlyLoading || $scope.finishedLoading)
                {
                    deferred.resolve(false); // return false to indicate caller shouldn't try again
                }
                else if ($scope.currentlyFiltering)
                {
                    deferred.resolve(true); // return true to indicate caller should try again
                }
                else
                {
                    $scope.currentlyLoading = true;

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/ReadLogItemIntros',
                        params: {
                            'appenderName' : appenderName,
                            'partitionKey' : lastPartitionKey != null ? escape(lastPartitionKey) : '',
                            'rowKey': lastRowKey != null ? escape(lastRowKey) : '',
                            'take': 50
                        },
                        data: queryFilters
                    })
                    .then(function (response) {

                        if (angular.isArray(response.data)) // success
                        {
                            if (response.data.length > 0) {

                                $scope.logItems = $scope.logItems.concat(response.data);

                                var lastLogItem = response.data[response.data.length - 1];

                                // last keys in response
                                lastPartitionKey = lastLogItem.partitionKey;
                                lastRowKey = lastLogItem.rowKey;
                            }
                            else {
                                $scope.finishedLoading = true;
                            }
                        }
                        else // timeout
                        {
                            // thre may have been items found, before it timed out
                            if (response.data.data.length > 0) {
                                $scope.logItems = $scope.logItems.concat(response.data.data);
                            }

                            // last keys checked before timeout occured
                            lastPartitionKey = response.data.lastPartitionKey;
                            lastRowKey = response.data.lastRowKey;
                        }

                        $scope.currentlyLoading = false;

                        deferred.resolve(!$scope.finishedLoading); // when true indicates the caller could try again
                    });
                }

                return deferred.promise;
            };

            $scope.toggleLogItemDetails = function ($event, logItem) {

                var logItemRow = $($event.currentTarget);
                var logItemDetailsRow = logItemRow.next();

                // update ui
                logItemRow.toggleClass('open');
                logItemDetailsRow.toggle();

                // get data if missing
                if (logItemDetailsRow.is(':visible') && logItem.details === undefined) {

                    // TODO: prevent multiple requests for the same data

                    $http({
                        method: 'GET',
                        url: 'BackOffice/AzureLogger/Api/ReadLogItemDetail',
                        params: {
                            appenderName: appenderName,
                            partitionKey: logItem.partitionKey,
                            rowKey: logItem.rowKey
                        }
                    })
                   .then(function (response) {
                       logItem.details = response.data;
                   });
                }

            };

            $scope.differentDays = function (logItem, lastLogItem) {

                if (lastLogItem === undefined) { return true; } // if there wasn't a last one

                var date = new Date(logItem.eventTimestamp);
                var lastDate = new Date(lastLogItem.eventTimestamp);

                return date.toDateString() !== lastDate.toDateString();
            };

        }]);
