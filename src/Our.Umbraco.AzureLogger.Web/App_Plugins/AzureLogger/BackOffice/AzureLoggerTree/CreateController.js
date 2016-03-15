angular
    .module('umbraco')
    .controller('AzureLogger.CreateController', [
        '$scope', 'navigationService', 'treeService', 'AzureLogger.SearchItemResource',
        function ($scope, navigationService, treeService,searchItemResource) {

            $scope.cancel = function () {
                navigationService.hideNavigation();
            };

            $scope.create = function (currentNode) {

                searchItemResource.createSearchItem(
                    $scope.name,
                    function () {

                        treeService.loadNodeChildren({ node: currentNode })
                        .then(function () {

                            // TODO: goto item in tree, and trigger it to update the main view

                            navigationService.hideNavigation();
                        });

                    });

            };

        }]);