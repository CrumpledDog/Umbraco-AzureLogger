﻿angular
    .module('umbraco')
    .controller('AzureLogger.ViewLogController', [
        '$scope', '$http', '$routeParams', 'AzureLogger.SearchFiltersResource',
        function ($scope, $http, $routeParams, searchFiltersResource) {

        /* scope vars */
        $scope.routeParams = $routeParams;

        $scope.connected = false;
        //$scope.connectionErrorMessage;
        //$scope.connectionString;
        //$scope.tableName;

        $scope.filter = {};
        $scope.filter.minLevel = 'Debug';
        $scope.filter.hostName = '';
        $scope.filter.loggerName = ''; // UX needs to see log list at the same time

        //$scope.startEventTimestamp; // set with date picker
        //$scope.threadIdentity; // built from AppDomainId + ProcessId + ThreadName (set by clicking in details view)


        $scope.logItems = [];
        //$scope.logItemLimit = 1000; // size of logItems array before it is reset (and a new start date time in the filter)
        $scope.currentlyLoading = false; // true when getting data awaiting a response to set
        $scope.finishedLoading = false; // true once server indicates that there is no more data

        $scope.searchFiltersState = searchFiltersResource.searchFiltersState;
        //$scope.$watch('searchFiltersResource.searchFiltersState', function () { // wouldn't bind
        $scope.$watch('searchFiltersState', function () {
            $scope.logItems = [];
            $scope.finishedLoading = false; // reset
            $scope.getMoreLogItems();
        }, true);



        // get connection details (triggering a new connection if required)
        $http.get('BackOffice/AzureLogger/Api/Connect')
            .then(function (response) {
                $scope.connected = response.data.connected;
                //$scope.connectionErrorMessage = response.data.connectionErrorMessage;
                //$scope.connectionString = response.data.connectionString;
                //$scope.tableName = response.data.tableName;
            });

        /* scope methods */

        $scope.getMoreLogItems = function () {

            // TODO: if item count < logItemLimit

            // only request, if there isn't already a request pending and there might be data to get
            if (!$scope.finishedLoading && !$scope.currentlyLoading) {
                $scope.currentlyLoading = true;

                // get last known rowKey from array
                var rowKey = null;
                if ($scope.logItems.length > 0) { rowKey = $scope.logItems[$scope.logItems.length - 1].rowKey; }


                // NOTE: using nulls to indicate no value instead of empty strings ''
                // converts to empty strings on api call to ensure method signature matches
                $http({
                    method: 'GET',
                    url: 'BackOffice/AzureLogger/Api/GetLogItemIntros',
                    params: {
                        minLevel: $scope.searchFiltersState.minLevel,
                        hostName: $scope.searchFiltersState.hostName != null ? escape($scope.searchFiltersState.hostName) : '',
                        loggerName: $scope.searchFiltersState.loggerName != null ? escape($scope.searchFiltersState.loggerName) : '',
                        rowKey: rowKey != null ? escape(rowKey) : '',
                        take: 200
                    }
                })
                .then(function (response) {
                    console.log(response.data.length);

                    if (response.data.length > 0) {
                        $scope.logItems = $scope.logItems.concat(response.data); // add new data to array
                    } else {
                        $scope.finishedLoading = true;
                    }

                    $scope.currentlyLoading = false;
                });
            }
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
                    url: 'BackOffice/AzureLogger/Api/GetLogItemDetail',
                    params: {
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


    }])
    .directive('lazyLoad', ['$timeout', function ($timeout) {

        return {
            restrict: 'A',
            link: function (scope, element, attrs) {

                // returns true if the element hasn't overlaped the bottom of the view
                var lazyLoad = function () {
                    var overlap = 500;
                    return (element.offset().top + element.height() < $(window).height() + overlap);
                };

                // initialize to ensure content is loaded to fill the initial screen (before any scroll activity)
                $timeout(function () {
                    //TODO: check the lazy load has filled the screen
                    if (lazyLoad()) {
                        scope.$apply(attrs.lazyLoad);
                    }
                });

                var scrollDelayTimer;
                // jQuery walk up tree from element to find div with class 'umb-scrollable', to hook into when this is scrolled
                $(element).closest('.umb-scrollable').bind('scroll', function () {

                    // cancel any previous timer and set new one
                    if (scrollDelayTimer) {
                        $timeout.cancel(scrollDelayTimer);
                    }

                    scrollDelayTimer = $timeout(function () {
                        if (lazyLoad()) {
                            scope.$apply(attrs.lazyLoad);
                        }
                    }, 250);

                });
            }
        }
    }]);