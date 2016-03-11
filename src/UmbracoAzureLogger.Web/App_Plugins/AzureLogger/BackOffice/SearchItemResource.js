angular
    .module('umbraco.resources')
    .factory('AzureLogger.SearchItemResource', [ // NOTE: named ...Resource (instead of ...Service or ...Factory) to avoid confusion
        '$http', '$q',
        function ($http, $q) {

            var searchItems = {}; // { name: '', { minLevel: 'DEBUG', hostName: null, loggerName: null } }

            return {

                searchItems: searchItems,
                /*
                    Creates a new search item (with empty filter properties) and persists to Azure table storage,
                    and then runs any callback
                */
                createSearchItem: function (name, callback) {
                    console.log('searchItemResource.createSearchItem(' + name + ')');

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/CreateSearchItem',
                        params: { name: name }
                    })
                    .then(function () { // TODO: need searchItemId returned

                        // TODO: update local data

                        if (typeof callback === 'function') { callback(); }

                    });

                },

                /*
                    Reads a search item (either from local array else falls back to requesting from Azure table storage),
                    and returns a promise
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
                    console.log('searchItemResource.updateSearchItem(' + searchItemId + ')');

                    $http({
                        method: 'POST',
                        url: 'BackOffice/AzureLogger/Api/UpdateSearchItem',
                        params: {
                            searchItemId: searchItemId,
                            minLevel: searchItem.minLevel != null ? searchItem.minLevel : 'DEBUG', // TEMP HACK FIX
                            hostName: searchItem.hostName != null ? escape(searchItem.hostName) : '',
                            loggerName: searchItem.loggerName != null ? escape(searchItem.loggerName) : ''
                        }
                    })
                    .then(function () {

                        //  update local data
                        searchItems[searchItemId] = searchItem;

                        if (typeof callback === 'function') { callback(); }

                    });

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

                       // remove from local data
                       searchItems[searchItemId] = null;

                       if (typeof callback === 'function') { callback(); }

                   });

                }


            } // end return
    }]);