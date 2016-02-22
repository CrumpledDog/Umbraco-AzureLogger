angular
    .module('umbraco')
    .controller('AzureLogger.DeleteController', [
        '$scope',
        function ($scope) {

            /* scope vars */

            /* scope methods */
            $scope.delete = function (nav) { // TODO: inject nav instead of passing from view ?
                console.log('delete clicked');

                // call api to delete azure record for this searchItem

                // remove filterState for this searchItem from the resource (if the view for this search item is active, it'll replace itself by retuning to the 'developer' section

                // refresh tree
            };

        }]);