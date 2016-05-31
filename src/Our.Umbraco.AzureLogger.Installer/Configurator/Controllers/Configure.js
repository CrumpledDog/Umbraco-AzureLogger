var configApp = angular.module('AzureLoggerLoader', []);

configApp.controller("Loader", function ($scope, $http, $log) {
    var postDataUrl = "/Umbraco/backoffice/AzureLogger/Installer/PostParameters";

    $scope.saved = false;
    // Default value
    $scope.connectionString = "DefaultEndpointsProtocol=https;AccountName=[myAccountName];AccountKey=[myAccountKey]";

    $scope.submitForm = function (e) {
        e.preventDefault();

        $http.post(postDataUrl, JSON.stringify($scope.connectionString)).success(function (data) {

            var status;
            if (typeof data === "string") {
                status = JSON.parse(data);
            } else {
                status = data;
            }

            $scope.status = status;

            if (status !== "ConnectionError") {
                $scope.saved = true;
            }

        });

    }

});
