angular
    .module('umbraco.resources')
    .factory('AzureLogger.SearchItemResource', [ // NOTE: named ...Resource (instead of ...Service or ...Factory) to avoid confusion
        '$http', '$q',
        function ($http, $q) {

            /* eg.
               searchItems = {
                searchItemId1: { name: 'Item 1', minLevel: 'DEBUG', hostName: null, loggerNamesInclude: false, loggerNames: [] }
                searchItemId2: { name: 'Item 2', minLevel: 'DEBUG', hostName: null, loggerNamesInclude: false, loggerNames: [] }
               }
            */
            var searchItems = {};

            return {

                searchItems: searchItems,


                /*
                    Reads a search item (either from local array else falls back to requesting from Azure table storage),
                    and returns a promise
                */
                readSearchItem: function (searchItemId) {

                    var deferred = $q.defer();

                    // TODO: look in local array for filters, if not there, then ajax request and populate local array

                    $http({
                        method: 'GET',
                        url: 'BackOffice/AzureLogger/Api/ReadSearchItem',
                        params: { searchItemId: searchItemId }
                    })
                    .then(function (response) {

                        // update local data // TODO: return the search id that was created
                        searchItems[searchItemId] = response.data;

                        deferred.resolve(response.data); // returns a searchItem

                    });

                    return deferred.promise;
                },

                /*
                    Update filter properties for a search item, persisting to Azure table storage,
                    and then runs any callback
                */
                updateSearchItem: function (searchItemId, searchItem, callback) { // TODO: move id into the searchItem obj

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/UpdateSearchItem',
                        params: { searchItemId: searchItemId },
                        data: searchItem
                    })
                    .then(function () {

                        //  update local data (this will trigger the watch in the view)
                        searchItems[searchItemId] = searchItem;

                        if (typeof callback === 'function') { callback(); }

                    });

                },

                /*
                    Delete a search item from Azure table storage (and local array)
                */
                deleteSearchItem: function (searchItemId, callback) {

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/DeleteSearchItem',
                        params: { searchItemId: searchItemId }
                    })
                   .then(function () {

                       // remove from local data
                       searchItems[searchItemId] = null;

                       if (typeof callback === 'function') { callback(); }

                   });

                }

            } // end return
    }]);