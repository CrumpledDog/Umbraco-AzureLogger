(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.ViewLogController', ViewLogController);

    ViewLogController.$inject = ['$scope', '$routeParams', 'navigationService', '$q', '$timeout', 'AzureLogger.AzureLoggerResource']

    function ViewLogController($scope, $routeParams, navigationService, $q, $timeout, azureLoggerResource) {

        var appenderName = $routeParams.id;
        var queryFilters = { hostName: '', loggerName: '', minLevel: '0', message: '', sessionId: '' };
        var lastPartitionKey = null; // partition key of last item checked in last query (where to start next query)
        var lastRowKey = null; // row key of last item checked in last query (where to start next query)

        $scope.logItems = [];
        $scope.machineNames = [];
        $scope.loggerNames = [];
        $scope.currentlyLoading = false; // true when getting data awaiting a response to set
        $scope.finishedLoading = false; // true once server indicates that there is no more data
        // TODO: $scope.startEventTimestamp; // set with date picker
        // TODO: $scope.threadIdentity; // built from AppDomainId + ProcessId + ThreadName (set by clicking in details view)
        // TODO: $scope.logItemLimit = 1000; // size of logItems array before it is reset (and a new start date time in the filter)
        $scope.uiFilters = angular.copy(queryFilters); // set the ui filter state to match
        $scope.currentlyFiltering = false;

        $scope.filtersMatch = filtersMatch;
        $scope.handleFilters = handleFilters;
        $scope.getMoreLogItems = getMoreLogItems;
        $scope.toggleLogItemDetails = toggleLogItemDetails;
        $scope.differentDays = differentDays;

        $scope.$on('WipedLog', wipedLog);

        init();

        // --------------------------------------------------------------------------------

        function wipedLog(event, arg) {
            // if the broadcast was intended for this appender
            if (arg == appenderName) {  clearLogItems(); }
        }

        function init() {
            // forces the tree to highlight appender used for this view
            // https://our.umbraco.org/forum/umbraco-7/developing-umbraco-7-packages/48870-Make-selected-node-in-custom-tree-appear-selected
            navigationService.syncTree({ tree: 'azureLoggerTree', path: ['-1', 'appender|' + appenderName], forceReload: false });

            // tell the resource that this is now the currently active view
            azureLoggerResource.activeAppenderViewLog = appenderName;

            azureLoggerResource.getIndexes(appenderName)
            .then(function (response) {
                $scope.machineNames = response.data.machineNames;
                $scope.loggerNames = response.data.loggerNames;
            });
        }

        function clearLogItems() {
            $scope.logItems = [];
            lastPartitionKey = null;
            lastRowKey = null;
            $scope.finishedLoading = false;
            triggerLazyLoad();
        }

        // tell lazy-load directive to fill screen
        function triggerLazyLoad() {
            $scope.lazyLoad(); // WARNING: directive added this method to scope (directive not intended to be reuseable)
        }

        // checks to see if the ui filters and the query filters represent the same state - returns bool
        function filtersMatch() {
            return angular.equals($scope.uiFilters, queryFilters);
        }

        // handles any filter ui changes - returns a promise
        function handleFilters() {

            var deferred = $q.defer();

            // if already filtering, or there's no update of the query filters required
            if ($scope.currentlyFiltering || $scope.filtersMatch()) {
                deferred.resolve();
            } else {
                $scope.currentlyFiltering = true;

                $timeout(function () { // timeout ensures scope is ready

                    var uiFilterHostName = $scope.uiFilters.hostName.toLowerCase();
                    var uiFilterLoggerName = $scope.uiFilters.loggerName.toLowerCase();
                    var uiFilterMinLevel = $scope.uiFilters.minLevel;
                    var uiFilterMessage = $scope.uiFilters.message.toLowerCase();
                    var uiFilterSessionId = $scope.uiFilters.sessionId;

                    var queryFilterHostName = queryFilters.hostName.toLowerCase();
                    var queryFilterLoggerName = queryFilters.loggerName.toLowerCase();
                    var queryFilterMinLevel = queryFilters.minLevel;
                    var queryFilterMessage = queryFilters.message.toLowerCase();
                    var queryFilterSessionId = queryFilters.sessionId;

                    queryFilters = angular.copy($scope.uiFilters); // update queryFilters as early as possible

                    // when true, indicates that the current result set will be reduced
                    var reductive = uiFilterHostName.indexOf(queryFilterHostName) > -1
                                    && uiFilterLoggerName.indexOf(queryFilterLoggerName) > -1
                                    && uiFilterMinLevel >= queryFilterMinLevel
                                    && uiFilterMessage == queryFilterMessage // any change in message will be reductive - as not all data client side
                                    && uiFilterSessionId == queryFilterSessionId; // any change in session id will be reductive - as not all data client side

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

                        // TODO: session reductive filtering (will require client side data)

                        // tell lazy-load directive to fill screen, as number of items may have been reduced
                        triggerLazyLoad();

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
        }

        // returns a promise with a bool result - the bool indicates whether the caller should try again
        function getMoreLogItems() {

            var deferred = $q.defer();

            if (appenderName != azureLoggerResource.activeAppenderViewLog || $scope.currentlyLoading || $scope.finishedLoading) {
                deferred.resolve(false); // return false to indicate caller shouldn't try again
            }
            else if ($scope.currentlyFiltering) {
                deferred.resolve(true); // return true to indicate caller should try again
            }
            else {
                $scope.currentlyLoading = true;

                azureLoggerResource.readLogItemIntros(appenderName, lastPartitionKey, lastRowKey, queryFilters)
                .then(function (response) {

                    $scope.logItems = $scope.logItems.concat(response.data.logItemIntros);
                    lastPartitionKey = response.data.lastPartitionKey;
                    lastRowKey = response.data.lastRowKey;
                    $scope.finishedLoading = response.data.finishedLoading;
                    $scope.currentlyLoading = false;

                    deferred.resolve(!$scope.finishedLoading); // when true indicates the caller could try again
                });
            }

            return deferred.promise;
        }

        function toggleLogItemDetails($event, logItem) {

            var logItemRow = $($event.currentTarget);
            var logItemDetailsRow = logItemRow.next();

            // update ui
            logItemRow.toggleClass('open');
            logItemDetailsRow.toggle();

            // get data if missing
            if (logItemDetailsRow.is(':visible') && logItem.details === undefined) {
                azureLoggerResource.readLogItemDetail(appenderName, logItem.partitionKey, logItem.rowKey)
               .then(function (response) {
                   logItem.details = response.data;
               });
            }
        }

        function differentDays(logItem, lastLogItem) {

            if (lastLogItem === undefined) { return true; } // if there wasn't a last one

            var date = new Date(logItem.eventTimestamp);
            var lastDate = new Date(lastLogItem.eventTimestamp);

            return date.toDateString() !== lastDate.toDateString();
        }

    }

})();