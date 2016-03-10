angular
    .module('umbraco')
    .controller('AzureLogger.DeleteController', [
        '$scope', 'navigationService', 'treeService', 'AzureLogger.SearchItemResource',
        function ($scope, navigationService, treeService, searchItemResource) {

            $scope.cancel = function () {
                navigationService.hideNavigation();
            };

            $scope.delete = function (currentNode) {

                searchItemResource.deleteSearchItem(
                    currentNode.id.split('|')[1], // strip the tree prefix
                    function () {
                        treeService.removeNode(currentNode); // refresh tree
                        navigationService.hideDialog();
                    });

            };

        }]);