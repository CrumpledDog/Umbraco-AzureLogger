(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.WipeLogController', WipeLogController);

    WipeLogController.$inject = ['$scope', 'navigationService', 'AzureLogger.AzureLoggerResource'];

    function WipeLogController($scope, navigationService, azureLoggerResource) {

        var appenderName;

        // init function used to get the tree node id from the view ! (as can't find a suitable resource to inject so as to get at this value)
        // the current node represents the tree node associated with the current menu option
        $scope.init = function (currentNode) {
            appenderName = currentNode.id.split('|')[1]; // strip the 'appender|' prefix
        };

        $scope.cancel = function () {
            navigationService.hideNavigation();
        };

        $scope.wipeLog = function () {

            // TODO: update ui to indicate operation taking place...

            azureLoggerResource.wipeLog(appenderName)
            .then(function () {
                navigationService.hideNavigation();
            });

        };
    }

})();