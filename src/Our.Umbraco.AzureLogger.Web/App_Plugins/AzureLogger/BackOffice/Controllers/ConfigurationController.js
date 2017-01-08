(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.ConfigurationController', ConfigurationController);

    ConfigurationController.$inject = ['$scope', '$location', 'navigationService', 'AzureLogger.AzureLoggerResource'];

    function ConfigurationController($scope, $location, navigationService, azureLoggerResource) {
      
        $scope.init = init;
 
        // --------------------------------------------------------------------------------

        function init(currentNode) {

            navigationService.hideMenu();

            //HACK: currentNode isn't supplied after the location change (this is to re-load this view into the main area)
            if (currentNode != undefined) {
                $location.path('/developer/azureLoggerTree/Configuration/FullView');
            }

        }

    }

})();