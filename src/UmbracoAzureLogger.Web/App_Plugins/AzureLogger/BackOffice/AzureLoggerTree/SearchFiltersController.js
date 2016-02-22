angular
    .module('umbraco')
    .controller('AzureLogger.SearchFiltersController', [
        '$scope', 'AzureLogger.SearchFiltersResource',
        function ($scope, searchFiltersResource) {

            /* scope vars */
            // $scope.searchFiltersState = searchFiltersResource.searchFiltersState; // this auto binds 2-way to the resource object
            // specifying properties to the primatives prevents 2-way binding, so that we can set all on save (else cancel changes)
            $scope.minLevel = searchFiltersResource.searchFiltersState.minLevel;
            $scope.hostName = searchFiltersResource.searchFiltersState.hostName;
            $scope.loggerName = searchFiltersResource.searchFiltersState.loggerName;

            /* scope methods */
            $scope.save = function(nav) { // TODO: inject nav instead of passing from view ?

                searchFiltersResource.searchFiltersState.minLevel = $scope.minLevel;
                searchFiltersResource.searchFiltersState.hostName = $scope.hostName;
                searchFiltersResource.searchFiltersState.loggerName = $scope.loggerName;

                console.log('saving search filters');
                console.log(searchFiltersResource.searchFiltersState);

                nav.hideDialog();
            };

    }]);