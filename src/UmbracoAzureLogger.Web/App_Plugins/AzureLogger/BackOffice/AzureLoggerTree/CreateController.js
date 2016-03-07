angular
    .module('umbraco')
    .controller('AzureLogger.CreateController', [
        '$scope', '$http',
        function ($scope, $http) {

            /* scope vars */

            /* scope methods */
            $scope.save = function (nav) { // TODO: inject nav instead of passing from view ?
                console.log('create clicked');


                $http({
                    method: 'POST',
                    url: 'BackOffice/AzureLogger/Api/Create',
                    params: {
                        name: $scope.name
                    }
                })
                .then(function (response) {

                    console.log('created...');

                    // refresh tree

                    // goto item in tree

                });
            };

        }]);