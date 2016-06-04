(function() {
    'use strict';

    angular
        .module('umbraco')
        .controller('AzureLogger.EditConfigFileController', EditConfigFileController);

    EditConfigFileController.$inject = ['$scope', '$http'];

    function EditConfigFileController($scope, $http) {

        /* vars */
        $scope.log4netConfigFile = null;

        /* startup */

        $http({
            method: 'GET',
            url: 'BackOffice/AzureLogger/Api/ReadLog4NetConfigFile'
        })
        .then(function (response) {
            $scope.log4netConfigFile = response.data;
            console.log('yes');
        });


        /* methods */


        $scope.save = function () {
            $http({
                method: 'POST',
                url: 'BackOffice/AzureLogger/Api/WriteLog4NetConfigFile',
                data: $scope.log4netConfigFile
            })
            .then(function () {

            });
        };

    }

})();