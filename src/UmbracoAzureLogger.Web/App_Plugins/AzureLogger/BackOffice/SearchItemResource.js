angular
    .module('umbraco.resources')
    .factory('AzureLogger.SearchItemResource', [ // NOTE: named ...Resource (instead of ...Service or ...Factory) to avoid confusion
        '$http',
        function ($http) {

            var searchItems = [];

            return {

                createSearchItem: function (name, callback) {
                    console.log('searchItemResource.createSearchItem(' + name + ')');

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/Create',
                        params: {
                            name: name
                        }
                    })
                    .then(function () {

                        if (typeof callback === 'function') { callback(); }

                    });

                },

                readSearchItem: function (searchItemId) {
                    console.log('searchItemResource.readSearchItem(' + searchItemId + ')');

                    // TODO: look in local array for filters, if not there, then ajax request and populate local array

                    // hardcoded debug - return fake searchItemFilterState obj
                    return { name: 'example search item name', minLevel: 'DEBUG', hostName: null, loggerName: null };
                },

                updateSearchItem: function (searchItemId, searchItemFilterState, callback) {
                    console.log('searchItemResource.updateSearchItem(' + searchItemId + ')');
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