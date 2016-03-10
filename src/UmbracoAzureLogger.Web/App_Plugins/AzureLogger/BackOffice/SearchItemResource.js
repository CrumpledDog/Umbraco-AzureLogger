angular
    .module('umbraco.resources')
    .factory('AzureLogger.SearchItemResource', [ // NOTE: named ...Resource (instead of ...Service or ...Factory) to avoid confusion
        '$http',
        function ($http) {

            var searchItems = [];

            return {

                getSearchItemFilterState: function (searchItemId) {
                    console.log('searchItemResource.getSearchItemFilterState(' + searchItemId + ')');

                    // TODO: look in local array for filters, if not there, then ajax request and populate local array

                    // hardcoded debug - return fake searchItemFilterState obj
                    return { minLevel: 'DEBUG', hostName: null, loggerName: null };
                },

                setSearchItemFilterState: function (searchItemId, searchItemFilterState, callback) {
                    console.log('searchItemResource.setSearchItemFilterState(' + searchItemId + ')');
                    // searchFiltersState: { minLevel: '', hostName: '', loggerName: '' }

                    // TODO: ajax call to persist data, then add / update array here

                    if (typeof callback === 'function') { callback(); }
                },

                deleteSearchItem: function (searchItemId, callback) {
                    console.log('searchItemResource.deleteSearchItem(' + searchItemId + ')');

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/Delete',
                        params: {
                            rowKey: searchItemId
                        }
                    })
                   .then(function () {

                       // TODO: then remove array here

                       if (typeof callback === 'function') { callback(); }

                   });

                }


            } // end return
    }]);