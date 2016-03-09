angular
    .module('umbraco')
    .controller('AzureLogger.SearchFiltersController', [
        '$scope', 'navigationService', 'AzureLogger.SearchFiltersResource',
        function ($scope, navigationService, searchFiltersResource) {

            // $scope.searchFiltersState = searchFiltersResource.searchFiltersState; // this auto binds 2-way to the resource object
            // specifying properties to the primatives prevents 2-way binding, so that we can set all on save (else cancel changes)
            $scope.minLevel = searchFiltersResource.searchFiltersState.minLevel;
            $scope.hostName = searchFiltersResource.searchFiltersState.hostName;
            $scope.loggerName = searchFiltersResource.searchFiltersState.loggerName;


            $scope.cancel = function () {
                navigationService.hideNavigation();
            };

            $scope.save = function (currentNode) {

                searchFiltersResource.searchFiltersState.minLevel = $scope.minLevel;
                searchFiltersResource.searchFiltersState.hostName = $scope.hostName;
                searchFiltersResource.searchFiltersState.loggerName = $scope.loggerName;

                navigationService.hideNavigation();
            };

    }]);