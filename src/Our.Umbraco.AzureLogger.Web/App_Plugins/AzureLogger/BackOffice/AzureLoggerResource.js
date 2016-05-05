(function() {
    'use strict';

    angular
        .module('umbraco')
        .factory('AzureLogger.AzureLoggerResource', AzzureLoggerResource);

    AzzureLoggerResource.$inject = ['$rootScope', '$http'];

    function AzzureLoggerResource($rootScope, $http) {

        var resource = {
            activeAppenderViewLog: null, // used to identify the appender currently being viewed
            wipeLog: wipeLog
        };

        return resource;

        // --------------------------------------------------------------------------------

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