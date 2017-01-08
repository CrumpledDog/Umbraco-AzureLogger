(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.ConfigurationController', ConfigurationController);

    ConfigurationController.$inject = ['$scope', 'AzureLogger.AzureLoggerResource'];

    function ConfigurationController($scope, azureLoggerResource) {
      
        $scope.init = init;
 
        // --------------------------------------------------------------------------------

        function init() {


        }

    }

})();