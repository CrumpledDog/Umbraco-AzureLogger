angular
    .module('umbraco')
    .controller('AzureLogger.SearchFiltersController', [
        '$scope', 'navigationService', 'AzureLogger.SearchItemResource',
        function ($scope, navigationService, searchItemResource) {

            var searchItemId; // the id for the searchItem (rowKey in Azure Table) with the tree 'searchItem|' prefix removed

            // init function used to get the tree node id from the view ! (as can't find a suitable resource to inject so as to get at this value)
            // the current node represents the tree node associated with the current search filters menu view
            $scope.init = function (currentNode) {
                // the tree node id as a 'searchItem' prefix (so controller can easily identify it's type and apply appropriate menu options)
                searchItemId = currentNode.id.split('|')[1]; // strip prefix

                // get filter state for this search item (from resource)
                searchItemResource.readSearchItem(searchItemId).then(function (searchItem) {
                    $scope.searchItem = searchItem;

                    // pulling into multiple vars so that auto-binding disabled, and update takes place when user clicks 'save'
                    $scope.minLevel = searchItem.minLevel;
                    $scope.hostName = searchItem.hostName;
                    $scope.loggerNamesInclude = searchItem.loggerNamesInclude;
                    $scope.loggerNames = searchItem.loggerNames;
                });
            }

            $scope.cancel = function () {
                navigationService.hideNavigation();
            };

            $scope.save = function () {

                $scope.searchItem.minLevel = $scope.minLevel;
                $scope.searchItem.hostName = $scope.hostName;
                $scope.searchItem.loggerNamesInclude = $scope.loggerNamesInclude;
                $scope.searchItem.loggerNames = $scope.loggerNames;

                searchItemResource.updateSearchItem(
                    searchItemId,
                    $scope.searchItem,
                    function () { navigationService.hideNavigation(); } // callback as persisting via ajax may take time
                );

            };

    }]);