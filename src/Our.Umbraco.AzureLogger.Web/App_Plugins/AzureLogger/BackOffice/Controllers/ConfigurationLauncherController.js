(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.ConfigurationLauncherController', ConfigurationLauncherController);

    ConfigurationLauncherController.$inject = ['$scope', '$location', 'navigationService'];

    /* by default, view results for menu actions are loaded into a side panel
       this is a hack, so that the "Configuration" menu action will load it's view into the main cms area */
    function ConfigurationLauncherController($scope, $location, navigationService) {

        $scope.init = init;

        function init() {

            navigationService.hideMenu();

            $location.path('/developer/azureLoggerTree/Configuration/-1');
        }


    }

})();