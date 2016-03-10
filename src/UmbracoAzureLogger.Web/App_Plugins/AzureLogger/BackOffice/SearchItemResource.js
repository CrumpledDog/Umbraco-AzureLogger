angular
    .module('umbraco.resources')
    .factory('AzureLogger.SearchItemResource', [function () { // NOTE: named ...Resource (instead of ...Service or ...Factory) to avoid confusion

        function parseId(id) {

            // TODO: parse 'searchItem|' prefix out if it exists
            console.log('parsing id');

            return id;
        }

        return {

            /*
            */
            getSearchItemFilterState: function (id) {

                // TODO: look in local array for filters, if not there, then ajax request and populate local array

            },

            setSearchItemFilterState: function (id, searchItemFilterState) {
                // searchFiltersState: { minLevel: '', hostName: '', loggerName: '' }

                // TODO: ajax call to persist data, then add / update array here
            },

            deleteSearchItem: function (id) {

                // TODO: ajax call to delete data, then remove from array here

            }

        }
    }]);