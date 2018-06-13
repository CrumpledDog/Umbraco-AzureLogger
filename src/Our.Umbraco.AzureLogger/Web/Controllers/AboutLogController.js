(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.AboutLogController', AboutLogController);

    AboutLogController.$inject = ['$scope', 'navigationService', 'AzureLogger.AzureLoggerResource'];

    function AboutLogController($scope, navigationService, azureLoggerResource) {

        var appenderName;

        $scope.init = init;
        $scope.appenderName = null;
        $scope.connectionString = null;
        $scope.tableName = null;
        $scope.readOnly = null;
        $scope.bufferSize = null;
        $scope.cancel = cancel;

        // --------------------------------------------------------------------------------

        // init function used to get the tree node id from the view ! (as can't find a suitable resource to inject so as to get at this value)
        // the current node represents the tree node associated with the current menu option
        function init(currentNode) {
            appenderName = currentNode.id.split('|')[1]; // strip the 'appender|' prefix
            load();
        }

        function load() {
            azureLoggerResource.getDetails(appenderName)
            .then(function (response) {

                $scope.appenderName = response.data.name;
                $scope.connectionString = response.data.connectionString;
                $scope.tableName = response.data.tableName;
                $scope.readOnly = response.data.readOnly;
                $scope.bufferSize = response.data.bufferSize;

            });
        }

        function cancel() {
            navigationService.hideNavigation();
        }
    }

})();