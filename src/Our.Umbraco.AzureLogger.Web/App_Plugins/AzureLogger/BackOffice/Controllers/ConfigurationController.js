(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.ConfigurationController', ConfigurationController);

    ConfigurationController.$inject = ['$scope', 'navigationService', 'AzureLogger.AzureLoggerResource'];

    function ConfigurationController($scope, navigationService, azureLoggerResource) {

        var appenderName;

        $scope.init = init;
 
        // --------------------------------------------------------------------------------

        function init() {
            
        }

    }

})();