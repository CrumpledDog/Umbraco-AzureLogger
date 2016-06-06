(function() {
    'use strict';

    /*
        <element lazy-load="method to call"></element>
    */
    angular
        .module('umbraco')
        .directive('lazyLoad', LazyLoadDirective);

    LazyLoadDirective.$inject = ['$timeout', 'AzureLogger.AzureLoggerResource'];

    function LazyLoadDirective($timeout, azureLoggerResource) {

        return {
            restrict: 'A',
            link: function (scope, element, attrs) {

                // 'locker'
                var expanding = false;

                // returns true if the element doesn't stretch below the bottom of the view
                var elementCanExpand = function () {
                    return (element.offset().top + element.height() < $(window).height() + 1000); // 1000 = number of pixels below view
                };

                // handles the 'method to call', and attempts to fill the screen
                var lazyLoad = function () {
                    expanding = true;

                    $timeout(function () { //timeout to ensure scope is ready

                        // TODO: to make more generic, check to see if a promise is actually returned
                        scope.$apply(attrs.lazyLoad) // execute angular expression string (the 'method to call')
                        .then(function (canLoadMore) { // return value of the promise

                            if (canLoadMore && elementCanExpand()) { // check to see if screen filled
                                lazyLoad(); // try again
                            }

                            expanding = false;
                        });
                    });
                };

                var previousScrollTop = 0;

                // jQuery find div with class 'umb-scrollable', as it's this outer element that is scrolled
                $(element).closest('.umb-scrollable').bind('scroll', function () {

                    // calculate direction of scroll
                    var currentScrollTop = $(this).scrollTop();
                    if (currentScrollTop <= previousScrollTop) { // up
                        // TODO: cancel any load
                    } else { // down
                        if (!expanding && elementCanExpand()) {
                            lazyLoad();
                        }
                    }
                    previousScrollTop = currentScrollTop;
                });

                // HACK: add method to the scope so can be called from controller (we know this is the only instance of the directive being used)
                scope.lazyLoad = function () {
                    if (!expanding && elementCanExpand()) { // safety check;
                        lazyLoad();
                    }
                };

                lazyLoad(); // startup
            }
        }
    }

})();