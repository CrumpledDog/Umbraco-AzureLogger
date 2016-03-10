angular
    .module('umbraco.resources')
    .factory('AzureLogger.SearchFiltersResource', [function () { // NOTE: named ...Resource (instead of ...Service or ...Factory) to avoid confusion
        return {

            searchFiltersState : {
                minLevel: 'DEBUG',
                hostName: null,
                loggerName: null
            }

        }
    }]);