(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.WipeLogController', WipeLogController);

    WipeLogController.$inject = ['$scope', 'navigationService', 'AzureLogger.AzureLoggerResource'];

    function WipeLogController($scope, navigationService, azureLoggerResource) {

        var appenderName;

        $scope.init = init;
        $scope.canel = cancel;
        $scope.wipeLog = wipeLog;

        // --------------------------------------------------------------------------------

        // init function used to get the tree node id from the view ! (as can't find a suitable resource to inject so as to get at this value)
        // the current node represents the tree node associated with the current menu option
        function init(currentNode) {
            appenderName = currentNode.id.split('|')[1]; // strip the 'appender|' prefix
        };

        function cancel() {
            navigationService.hideNavigation();
        };

        function wipeLog() {
            // TODO: update ui to indicate operation taking place...
            azureLoggerResource.wipeLog(appenderName)
            .then(function () {
                navigationService.hideNavigation();
            });
        };

    }

})();