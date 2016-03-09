angular
    .module('umbraco')
    .controller('AzureLogger.CreateController', [
        '$scope', 'navigationService', 'treeService', '$http',
        function ($scope, navigationService, treeService, $http) {

            $scope.cancel = function () {
                navigationService.hideNavigation();
            };

            $scope.create = function (currentNode) {

                $http({
                    method: 'POST',
                    url: 'BackOffice/AzureLogger/Api/Create',
                    params: {
                        name: $scope.name
                    }
                })
                .then(function (response) {

                    // refresh tree
                    treeService.loadNodeChildren({ node: currentNode })
                    .then(function () {

                        // TODO: goto item in tree, and trigger it to update the main view

                        navigationService.hideNavigation();
                    });

                });
            };

        }]);