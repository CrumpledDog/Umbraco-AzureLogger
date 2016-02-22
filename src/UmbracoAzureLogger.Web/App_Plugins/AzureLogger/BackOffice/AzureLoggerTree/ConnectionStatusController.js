angular
    .module('umbraco')
    .controller('AzureLogger.ConnectionStatusController', ['$scope', '$http', function ($scope, $http) {

        /* scope vars */

        $scope.connecting = true;
        $scope.connected = false;
        $scope.connectionErrorMessage;
        $scope.connectionString;
        $scope.tableName;

        // get connection details (triggering a new connection if required)
        $http.get('BackOffice/AzureLogger/Api/Connect')
            .then(function (response) {
                $scope.connected = response.data.connected;
                $scope.connectionErrorMessage = response.data.connectionErrorMessage;
                $scope.connectionString = response.data.connectionString;
                $scope.tableName = response.data.tableName;
            });

        /* scope methods */

    }]);