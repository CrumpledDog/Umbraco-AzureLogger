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

            //$scope.filters = {}; // the current ui filter state. { hostName: '', loggerName: '', messageIntro: '' }
            var queryFilters = { hostName: '', loggerName: '', messageIntro: '' }; // the filter state for the current query (may differ from the ui filters)
            $scope.filters = queryFilters;
            $scope.currentlyFiltering = false;

            $scope.debugQueryFilters = queryFilters;

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
            $scope.filtersChanged = function () {

                console.log('filters changed');
                $scope.currentlyFiltering = true;

                $timeout(function () { // HACK: timeout ensures scope is ready // TODO: change timout to simple promise

                    // if new value, contains old value then true (filter searching is anywhere in string)
                    var reductive = ($scope.filters.hostName.toLowerCase().indexOf(queryFilters.hostName.toLowerCase()) > -1)
                        && ($scope.filters.loggerName.toLowerCase().indexOf(queryFilters.loggerName.toLowerCase()) > -1)
                        && ($scope.filters.messageIntro.toLowerCase().indexOf(queryFilters.messageIntro.toLowerCase()) > -1);



                    if (reductive) {
                        // delete items that don't match, a new query may happen
                        console.log('reductive');

                        // has machine name changed ?
                        if ($scope.filters.hostName != queryFilters.hostName) {
                            console.log('hostname changed');

                            $scope.logItems = $scope.logItems.filter(function (value) {
                                return value.hostName.toLowerCase().indexOf($scope.filters.hostName.toLowerCase()) > -1;
                            });
                        }

                        // has logger name changed ?
                        if ($scope.filters.loggerName != queryFilters.loggerName) {
                            console.log('loggername changed');

                            $scope.logItems = $scope.logItems.filter(function (value) {
                                return value.loggerName.toLowerCase().indexOf($scope.filters.loggerName.toLowerCase()) > -1;
                            });
                        }

                        // has message changed ?
                        if ($scope.filters.messageIntro != queryFilters.messageIntro) {
                            console.log('messageintro changed');

                            $scope.logItems = $scope.logItems.filter(function (value) {
                                return value.messageIntro.toLowerCase().indexOf($scope.filters.messageIntro.toLowerCase()) > -1;
                            });
                        }

                    } else {
                        console.log('non-reductive');
                        clearLogItems(); // delete all items as we may be missing data (this will trigger a refresh)
                    }

                    queryFilters = angular.copy($scope.filters); // copy to prevent referencing

                }) // no delay in timeout (TODO: change to promise)
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

                // only request, if there isn't already a request pending and there might be data to get
                if (!$scope.finishedLoading && !$scope.currentlyLoading && appenderName == azureLoggerResource.activeAppenderViewLog) {

                    $scope.currentlyLoading = true;

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/ReadLogItemIntros',
                        params: {
                            'appenderName' : appenderName,
                            'partitionKey' : lastPartitionKey != null ? escape(lastPartitionKey) : '',
                            'rowKey': lastRowKey != null ? escape(lastRowKey) : '',
                            'take': 5
                        },
                        data: queryFilters
                    })
                    .then(function (response) {

                        if (angular.isArray(response.data)) // success, response may have new log items
                        {
                            if (response.data.length > 0) {

                                $scope.logItems = $scope.logItems.concat(response.data); // add new data to array

                                var lastLogItem = $scope.logItems[$scope.logItems.length - 1];

                                lastPartitionKey = lastLogItem.partitionKey;
                                lastRowKey = lastLogItem.rowKey;

                            } else {
                                $scope.finishedLoading = true;
                            }
                        }
                        else // timeout, response may have new log items, but it also has partion & row keys to indicate where next to start
                        {
                            if (response.data.data.length > 0) {
                                $scope.logItems = $scope.logItems.concat(response.data.data); // add new data to array
                            }

                            lastPartitionKey = response.data.lastPartitionKey;
                            lastRowKey = response.data.lastRowKey;
                        }

                        $scope.currentlyLoading = false;

                        deferred.resolve(!$scope.finishedLoading); // when true indicates the caller could try again
                    });
                }
                else
                {
                    deferred.resolve(false); // return false to indicate caller shouldn't try again
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
