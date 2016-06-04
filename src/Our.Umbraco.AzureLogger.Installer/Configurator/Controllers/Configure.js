var configApp = angular.module('AzureLoggerLoader', []);

configApp.controller("Loader", function ($scope, $http) {
  var getDataUrl = "/Umbraco/backoffice/AzureLogger/Installer/GetConnectionString";
  var postDataUrl = "/Umbraco/backoffice/AzureLogger/Installer/PostConnectionString";

  $scope.saved = false;

  $http.get(getDataUrl).success(function (data) {
    if (typeof data === "string") {
      $scope.connectionString = JSON.parse(data);
    } else {
      $scope.connectionString = data;
    }
  });

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
