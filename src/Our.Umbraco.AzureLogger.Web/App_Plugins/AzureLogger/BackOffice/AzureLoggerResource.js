(function() {
    'use strict';

    angular
        .module('umbraco')
        .factory('AzureLogger.AzureLoggerResource', AzzureLoggerResource);

    function AzzureLoggerResource() {

        return {

            activeAppenderViewLog: null // used to identify the appender currently being viewed

        };
    }

})();