angular
    .module('umbraco.resources')
    .factory('AzureLogger.SearchItemResource', [function () { // NOTE: named ...Resource (instead of ...Service or ...Factory) to avoid confusion
        return {

            getSearchItemFilterState: function () {

            },

            setSearchFilterState: function (searchItemFilterState) {
                // searchFiltersState: { minLevel: '', hostName: '', loggerName: '' }

                // TODO: ajax call to persist data, then add / update array here
            },

            deleteSearchItem: function () {

            }

        }
    }]);