angular
    .module('umbraco')
    .controller('AzureLogger.CreateController', [
        '$scope', '$http',
        function ($scope, $http) {

            /* scope vars */

            /* scope methods */
            $scope.save = function (nav) { // TODO: inject nav instead of passing from view ?
                console.log('create clicked');

                // is there already a searchItem with this name already ?

                // call api to create new azure record for this searchItem

                // refresh tree

                // goto item in tree
            };

        }]);