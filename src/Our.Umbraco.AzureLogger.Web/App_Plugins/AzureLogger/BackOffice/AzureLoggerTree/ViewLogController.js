angular
    .module('umbraco')
    .controller('AzureLogger.ViewLogController', [
        '$scope', '$http', '$routeParams', 'navigationService', 'AzureLogger.SearchItemResource',
        function ($scope, $http, $routeParams, navigationService, searchItemResource) {

            var searchItemId = $routeParams.id;

            $scope.init = function () {
                searchItemResource.readSearchItem(searchItemId).then(function (searchItem) {

                    //if (searchItem == null) {
                    //    // search item has been deleted from the tree, so reset this view to the main dev area
                    //    navigationService.changeSection('developer');

                    //} else {
                    $scope.searchItem = searchItem; // put into scope so view can delay rendering until populated

                    $scope.logItems = [];
                    $scope.finishedLoading = false;
                    $scope.getMoreLogItems();
                    //}

                });
            };

            // watch the search items collection in the resource, and trigger init if this search item data is changed
            $scope.searchItems = searchItemResource.searchItems;
            $scope.$watch('searchItems["' + searchItemId + '"]', function () {
                $scope.init();
            }, true);

            // forces the tree to highlight (in blue) the associated search item for this view
            // https://our.umbraco.org/forum/umbraco-7/developing-umbraco-7-packages/48870-Make-selected-node-in-custom-tree-appear-selected
            navigationService.syncTree({ tree: 'azureLoggerTree', path: ['-1', 'searchItem|' + searchItemId], forceReload: false });

            $scope.connected = false;
            //$scope.connectionErrorMessage;
            //$scope.connectionString;
            //$scope.tableName;

            //$scope.startEventTimestamp; // set with date picker
            //$scope.threadIdentity; // built from AppDomainId + ProcessId + ThreadName (set by clicking in details view)

            //            $scope.logItems = [];
            //$scope.logItemLimit = 1000; // size of logItems array before it is reset (and a new start date time in the filter)
            //            $scope.currentlyLoading = false; // true when getting data awaiting a response to set
            //            $scope.finishedLoading = false; // true once server indicates that there is no more data


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

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/ReadLogItemIntros',
                        params: { 'rowKey': rowKey != null ? escape(rowKey) : '', 'take': 200 },
                        data: $scope.searchItem // supply the full search item data
                    })
                    .then(function (response) {

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
                        url: 'BackOffice/AzureLogger/Api/ReadLogItemDetail',
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

        }]);
