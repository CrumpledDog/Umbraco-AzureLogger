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

                // TODO: remove filterState for this searchItem from the resource (if the view for this search item is active, it'll replace itself by retuning to the 'developer' section)

                $http({
                    method: 'POST',
                    url: 'BackOffice/AzureLogger/Api/Delete',
                    params: {
                        rowKey: currentNode.id.split('|')[1] //currentNode.id = "searchItem|" + x.RowKey
                    }
                })
                .then(function (response) {

                    // refresh tree
                    treeService.removeNode(currentNode);
                    navigationService.hideDialog();

                });
            };

        }]);