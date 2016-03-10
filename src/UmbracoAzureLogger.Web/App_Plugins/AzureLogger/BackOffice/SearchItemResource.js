angular
    .module('umbraco.resources')
    .factory('AzureLogger.SearchItemResource', [function () { // NOTE: named ...Resource (instead of ...Service or ...Factory) to avoid confusion

        var searchItems = [];

        return {

            getSearchItemFilterState: function (id) {
                console.log('searchItemResource.getSearchItemFilterState(' + id + ')');

                // TODO: look in local array for filters, if not there, then ajax request and populate local array

                // hardcoded debug
                return { minLevel: 'DEBUG', hostName: null, loggerName: null };
            },

            setSearchItemFilterState: function (id, searchItemFilterState) {
                console.log('searchItemResource.setSearchItemFilterState(' + id + ')');
                // searchFiltersState: { minLevel: '', hostName: '', loggerName: '' }

                // TODO: ajax call to persist data, then add / update array here
            },

            deleteSearchItem: function (id) {
                console.log('searchItemResource.deleteSearchItem(' + id + ')');
                // TODO: ajax call to delete data, then remove from array here
            }

        }
    }]);