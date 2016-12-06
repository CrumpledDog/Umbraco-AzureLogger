(function() {
    'use strict';

    angular
        .module('umbraco')
        .factory('AzureLogger.AzureLoggerResource', AzzureLoggerResource);

    AzzureLoggerResource.$inject = ['$rootScope', '$http', '$q'];

    function AzzureLoggerResource($rootScope, $http, $q) {

        var readLogItemIntrosCanceller = null; // promise used to be able to cancel long running http requests for 'readLogItemIntros'

        var resource = {
            activeAppenderViewLog: null, // identify the appender currently being viewed
            getIndexes: getIndexes,
            readLogItemIntros: readLogItemIntros,
            cancelReadLogItemIntros: cancelReadLogItemIntros,
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

            cancelReadLogItemIntros();

            readLogItemIntrosCanceller = $q.defer();

            return $http({
                method: 'POST',
                url: 'BackOffice/AzureLogger/Api/ReadLogItemIntros',
                params: {
                    'appenderName': appenderName,
                    'partitionKey': partitionKey != null ? escape(partitionKey) : '',
                    'rowKey': rowKey != null ? escape(rowKey) : ''                    
                },
                data: queryFilters,
                timeout: readLogItemIntrosCanceller.promise
            });
        }

        /* cancel any http request for readLogItemItros() */
        function cancelReadLogItemIntros() {
            if (readLogItemIntrosCanceller != null) {
                readLogItemIntrosCanceller.resolve();
                readLogItemIntrosCanceller = null;
            }
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