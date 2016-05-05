(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.ViewLogController', [
            '$scope', '$http', '$routeParams', 'navigationService', '$q', '$timeout', 'AzureLogger.AzureLoggerResource',
            function ($scope, $http, $routeParams, navigationService, $q, $timeout, azureLoggerResource) {

                /* vars */

                var appenderName = $routeParams.id;
                $scope.logItems = [];
                $scope.currentlyLoading = false; // true when getting data awaiting a response to set
                $scope.finishedLoading = false; // true once server indicates that there is no more data
                // TODO: $scope.startEventTimestamp; // set with date picker
                // TODO: $scope.threadIdentity; // built from AppDomainId + ProcessId + ThreadName (set by clicking in details view)
                // TODO: $scope.logItemLimit = 1000; // size of logItems array before it is reset (and a new start date time in the filter)

                var queryFilters = { hostName: '', loggerName: '', minLevel: -1, message: '' }; // the filter state to use for the ajax queries (-1 to ensure the dropdown placeholder is selected)
                $scope.uiFilters = angular.copy(queryFilters); // set the ui filter state to match
                $scope.currentlyFiltering = false;

                // partition key and row key of last item checked in last query (where to start next query)
                var lastPartitionKey = null;
                var lastRowKey = null;


                /* startup */

                // forces the tree to highlight appender used for this view
                // https://our.umbraco.org/forum/umbraco-7/developing-umbraco-7-packages/48870-Make-selected-node-in-custom-tree-appear-selected
                navigationService.syncTree({ tree: 'azureLoggerTree', path: ['-1', 'appender|' + appenderName], forceReload: false });

                // tell the resource that this is now the currently active view
                azureLoggerResource.activeAppenderViewLog = appenderName;

                /* methods */

                var clearLogItems = function () {
                    $scope.logItems = [];
                    lastPartitionKey = null;
                    lastRowKey = null;
                    $scope.finishedLoading = false;
                    triggerLazyLoad(); // local helper
                };

                // tell lazy-load directive to fill screen
                var triggerLazyLoad = function () {
                    $scope.lazyLoad(); // WARNING: directive added this method to scope (directive not intended to be reuseable)
                };

                // checks to see if the ui filters and the query filters represent the same state - returns bool
                $scope.filtersMatch = function ()
                {
                    // can't do a simple object compare - angular.equals($scope.uiFilters, queryFilters)
                    // as level drop down can be -1 (placeholder) or 0 (debug) - both values mean the same thing
                    return $scope.uiFilters.hostName == queryFilters.hostName &&
                           $scope.uiFilters.loggerName == queryFilters.loggerName &&
                           (
                                $scope.uiFilters.minLevel == queryFilters.minLevel ||
                                ($scope.uiFilters.minLevel == -1 && queryFilters.minLevel == 0) ||
                                ($scope.uiFilters.minLevel == 0 && queryFilters.minLevel == -1)
                           ) &&
                           $scope.uiFilters.message == queryFilters.message;
                };

                // handles any filter ui changes - returns a promise
                $scope.handleFilters = function () {

                    var deferred = $q.defer();

                    // if already filtering, or there's no update of the query filters required
                    if ($scope.currentlyFiltering || $scope.filtersMatch()) {
                        deferred.resolve();
                    }
                    else {

                        $scope.currentlyFiltering = true;

                        $timeout(function () { // timeout ensures scope is ready

                            var uiFilterHostName = $scope.uiFilters.hostName.toLowerCase();
                            var uiFilterLoggerName = $scope.uiFilters.loggerName.toLowerCase();
                            var uiFilterMinLevel = $scope.uiFilters.minLevel;
                            var uiFilterMessage = $scope.uiFilters.message.toLowerCase();

                            var queryFilterHostName = queryFilters.hostName.toLowerCase();
                            var queryFilterLoggerName = queryFilters.loggerName.toLowerCase();
                            var queryFilterMinLevel = queryFilters.minLevel;
                            var queryFilterMessage = queryFilters.message.toLowerCase();

                            queryFilters = angular.copy($scope.uiFilters); // update queryFilters as early as possible

                            // when true, indicates that the current result set will be reduced
                            var reductive = uiFilterHostName.indexOf(queryFilterHostName) > -1
                                            && uiFilterLoggerName.indexOf(queryFilterLoggerName) > -1
                                            && (uiFilterMinLevel >= queryFilterMinLevel || queryFilterMinLevel == 0)
                                            && uiFilterMessage == queryFilterMessage; // any change in message will be reductive - as not all data client side

                            // if reductive, then remove items that don't match (a new query may be triggered by the lazy load directive)
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
                                if (uiFilterMinLevel != queryFilterMinLevel) {
                                    $scope.logItems = $scope.logItems.filter(function (value) {
                                        return value.levelValue >= uiFilterMinLevel;
                                    });
                                }

                                // tell lazy-load directive to fill screen, as number of items may have been reduced
                                triggerLazyLoad(); // local helper

                            } else {
                                clearLogItems(); // delete all items as we may be missing data (this will trigger a refresh)
                            }

                        }) // no delay in timeout
                        .then(function () {
                            $scope.currentlyFiltering = false;
                            deferred.resolve(); // promise so caller can do somthing else when this has completed
                        });
                    }

                    return deferred.promise;
                };

                // listen for any 'WipedLog' broadcasts
                $scope.$on('WipedLog', function (event, arg) {
                    if (arg == appenderName) { // safety check: if destined for this appender
                        clearLogItems();
                    }
                });


                // returns a promise with a bool result - the bool indicates whether the caller should try again
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

})();