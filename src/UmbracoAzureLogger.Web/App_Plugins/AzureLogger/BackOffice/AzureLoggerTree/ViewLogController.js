angular
    .module('umbraco')
    .controller('AzureLogger.ViewLogController', [
        '$scope', '$http', '$routeParams', 'navigationService', 'AzureLogger.SearchItemResource',
        function ($scope, $http, $routeParams, navigationService, searchItemResource) {

            var searchItemId = $routeParams.id;

            searchItemResource.readSearchItem(searchItemId).then(function (searchItem) {
                $scope.searchItem = searchItem; // put into scope so view can delay rendering until populated
            });

            // forces the tree to highlight (in blue) the associated search item for this view
            // https://our.umbraco.org/forum/umbraco-7/developing-umbraco-7-packages/48870-Make-selected-node-in-custom-tree-appear-selected
            navigationService.syncTree({ tree: 'azureLoggerTree', path: ['-1', 'searchItem|' + searchItemId], forceReload: false });

            $scope.connected = false;
            //$scope.connectionErrorMessage;
            //$scope.connectionString;
            //$scope.tableName;

            //$scope.startEventTimestamp; // set with date picker
            //$scope.threadIdentity; // built from AppDomainId + ProcessId + ThreadName (set by clicking in details view)

            $scope.logItems = [];
            //$scope.logItemLimit = 1000; // size of logItems array before it is reset (and a new start date time in the filter)
            $scope.currentlyLoading = false; // true when getting data awaiting a response to set
            $scope.finishedLoading = false; // true once server indicates that there is no more data

            //$scope.$watch('searchFiltersResource.searchFiltersState', function () { // wouldn't bind
            //$scope.$watch('searchFiltersState', function () {
            //    $scope.logItems = [];
            //    $scope.finishedLoading = false; // reset
            //    $scope.getMoreLogItems();
            //}, true);



            // get connection details (triggering a new connection if required)
            $http.get('BackOffice/AzureLogger/Api/Connect')
                .then(function (response) {
                    $scope.connected = response.data.connected;
                    //$scope.connectionErrorMessage = response.data.connectionErrorMessage;
                    //$scope.connectionString = response.data.connectionString;
                    //$scope.tableName = response.data.tableName;
                });


            $scope.getMoreLogItems = function () {

                // TODO: if item count < logItemLimit

                // only request, if there isn't already a request pending and there might be data to get
                if (!$scope.finishedLoading && !$scope.currentlyLoading) {
                    $scope.currentlyLoading = true;

                    // get last known rowKey from array
                    var rowKey = null;
                    if ($scope.logItems.length > 0) { rowKey = $scope.logItems[$scope.logItems.length - 1].rowKey; }

                    console.log('the...');
                    console.log($scope.searchItem);

                    // NOTE: using nulls to indicate no value instead of empty strings ''
                    // converts to empty strings on api call to ensure method signature matches
                    $http({
                        method: 'GET',
                        url: 'BackOffice/AzureLogger/Api/GetLogItemIntros',
                        params: {
                            minLevel: $scope.searchItem.minLevel != null ? $scope.searchItem.minLevel : 'DEBUG', // TEMP HACK FIX
                            hostName: $scope.searchItem.hostName != null ? escape($scope.searchItem.hostName) : '',
                            loggerName: $scope.searchItem.loggerName != null ? escape($scope.searchItem.loggerName) : '',
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