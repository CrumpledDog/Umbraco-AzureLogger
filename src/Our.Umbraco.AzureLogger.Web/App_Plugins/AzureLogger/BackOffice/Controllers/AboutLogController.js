(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.AboutLogController', AboutLogController);

    AboutLogController.$inject = ['$scope', 'AzureLogger.AzureLoggerResource'];

    function AboutLogController($scope, azureLoggerResource) {

        var appenderName;

        $scope.init = init;

        // --------------------------------------------------------------------------------

        // init function used to get the tree node id from the view ! (as can't find a suitable resource to inject so as to get at this value)
        // the current node represents the tree node associated with the current menu option
        function init(currentNode) {
            appenderName = currentNode.id.split('|')[1]; // strip the 'appender|' prefix


        };
    }

})();