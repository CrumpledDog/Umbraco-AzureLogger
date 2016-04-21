angular
    .module('umbraco')
    .controller('AzureLogger.TrimLogController', [
        '$rootScope', '$scope', '$http', 'navigationService',
        function ($rootScope, $scope, $http, navigationService) {

            var appenderName;

            // init function used to get the tree node id from the view ! (as can't find a suitable resource to inject so as to get at this value)
            // the current node represents the tree node associated with the current menu option
            $scope.init = function (currentNode) {
                appenderName = currentNode.id.split('|')[1]; // strip the 'appender|' prefix
            };

            $scope.cancel = function () {
                navigationService.hideNavigation();
            };

            $scope.trimLog = function () {

                // TODO: update ui to indicate operation taking place...

                $http({
                    method: 'POST',
                    url: 'BackOffice/AzureLogger/Api/TrimLog',
                    params: { 'appenderName': appenderName }
                })
                .then(function () {

                    $rootScope.$broadcast('TrimmedLog', appenderName);

                    navigationService.hideNavigation();
                });

            };

        }]);

