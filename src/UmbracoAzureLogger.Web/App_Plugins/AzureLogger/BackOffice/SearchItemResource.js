angular
    .module('umbraco.resources')
    .factory('AzureLogger.SearchItemResource', [ // NOTE: named ...Resource (instead of ...Service or ...Factory) to avoid confusion
        '$http', '$q',
        function ($http, $q) {

            var searchItems = [];

            return {

                /*
                    Creates a new search item (with empty filter properties) and persists to Azure table storage
                */
                createSearchItem: function (name, callback) {
                    console.log('searchItemResource.createSearchItem(' + name + ')');

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/CreateSearchItem',
                        params: { name: name }
                    })
                    .then(function () {

                        if (typeof callback === 'function') { callback(); }

                    });

                },

                /*
                    Reads a search item (either from local array else falls back to requesting from Azure table storage)
                */
                readSearchItem: function (searchItemId) {
                    console.log('searchItemResource.readSearchItem(' + searchItemId + ')');

                    var deferred = $q.defer();

                    // TODO: look in local array for filters, if not there, then ajax request and populate local array

                    $http({
                        method: 'GET',
                        url: 'BackOffice/AzureLogger/Api/ReadSearchItem',
                        params: { searchItemId: searchItemId }
                    })
                    .then(function (response) {

                        // TODO: add to local array

                        deferred.resolve(response.data); // returns a searchItem

                    });

                    return deferred.promise;
                },

                /*
                    Update filter properties for a search item, persisting to Azure table storage
                */
                updateSearchItem: function (searchItemId, searchItem, callback) { // TODO: move id into the searchItem obj
                    console.log('searchItemResource.updateSearchItem(' + searchItemId + ')');





                    // TODO: ajax call to persist data, then add / update array here

                    if (typeof callback === 'function') { callback(); }
                },

                /*
                    Delete a search item from Azure table storage (and local array)
                */
                deleteSearchItem: function (searchItemId, callback) {
                    console.log('searchItemResource.deleteSearchItem(' + searchItemId + ')');

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/DeleteSearchItem',
                        params: { searchItemId: searchItemId }
                    })
                   .then(function () {

                       // TODO: then remove array here

                       if (typeof callback === 'function') { callback(); }

                   });

                }


            } // end return
    }]);