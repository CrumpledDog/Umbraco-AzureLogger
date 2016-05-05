(function() {
    'use strict';

    angular
        .module('umbraco')
        .factory('AzureLogger.AzureLoggerResource', [
            function () {

                return {

                    activeAppenderViewLog: null // used to identify the appender currently being viewed

                };

            }
        ]);

})();