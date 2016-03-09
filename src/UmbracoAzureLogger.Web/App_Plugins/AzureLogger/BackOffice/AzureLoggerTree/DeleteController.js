angular
    .module('umbraco')
    .controller('AzureLogger.DeleteController', [
        '$scope', 'navigationService', 'treeService', '$http',
        function ($scope, navigationService, treeService, $http) {

            $scope.cancel = function () {

                // both the following methods seem to behave in the same way
                //navigationService.hideNavigation();
                navigationService.hideDialog();
            };

            $scope.delete = function (currentNode) {

                // TODO: call api to delete azure record for this searchItem (currentNode.id = rowKey)

                // TODO: remove filterState for this searchItem from the resource (if the view for this search item is active, it'll replace itself by retuning to the 'developer' section)

                // refresh tree
                treeService.removeNode(currentNode);
                navigationService.hideDialog();
            };

        }]);