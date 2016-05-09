(function() {
    'use strict';

    angular
        .module('umbraco')
        .factory('AzureLogger.AzureLoggerResource', AzzureLoggerResource);

    AzzureLoggerResource.$inject = ['$rootScope', '$http'];

    function AzzureLoggerResource($rootScope, $http) {

        var resource = {
            activeAppenderViewLog: null, // identify the appender currently being viewed
            getIndexes: getIndexes,
            readLogItemIntros: readLogItemIntros,
            readLogItemDetail: readLogItemDetail,
            wipeLog: wipeLog
        };

        return resource;

        // --------------------------------------------------------------------------------

        function getIndexes(appenderName) {
            return $http({
                method: 'GET',
                url: 'BackOffice/AzureLogger/Api/GetIndexes',
                params: {
                    'appenderName': appenderName
                }
            });
        }

        function readLogItemIntros(appenderName, partitionKey, rowKey, queryFilters) {
            return $http({
                method: 'POST',
                url: 'BackOffice/AzureLogger/Api/ReadLogItemIntros',
                params: {
                    'appenderName': appenderName,
                    'partitionKey': partitionKey != null ? escape(partitionKey) : '',
                    'rowKey': rowKey != null ? escape(rowKey) : '',
                    'take': 50
                },
                data: queryFilters
            });
        }

        function readLogItemDetail(appenderName, partitionKey, rowKey) {
            return $http({
                method: 'GET',
                url: 'BackOffice/AzureLogger/Api/ReadLogItemDetail',
                params: {
                    'appenderName': appenderName,
                    'partitionKey': partitionKey,
                    'rowKey': rowKey
                }
            });
        }

        function wipeLog(appenderName) {
            return $http({
                method: 'POST',
                url: 'BackOffice/AzureLogger/Api/WipeLog',
                params: { 'appenderName': appenderName }
            })
            .then(function () {
                $rootScope.$broadcast('WipedLog', appenderName); // TODO: remove broadcast and use property on this resource instead
            });
        }
    }

})();